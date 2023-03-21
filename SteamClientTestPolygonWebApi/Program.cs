using System.Net;
using System.Reflection;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Refit;
using SteamClientTestPolygonWebApi.Application.Common;
using SteamClientTestPolygonWebApi.Application.Features.Inventory.TradeCooldownParsers;
using SteamClientTestPolygonWebApi.Application.SteamRemoteServices;
using SteamClientTestPolygonWebApi.Contracts.External;
using SteamClientTestPolygonWebApi.Helpers.Refit;
using SteamClientTestPolygonWebApi.Infrastructure.Persistence;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.BackGroundServices;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker.ProxyAnonymityJudges;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker.ProxySources;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker.ProxySources.GoodProxiesRu;
using SteamClientTestPolygonWebApi.Infrastructure.SteamRefitClients;

namespace SteamClientTestPolygonWebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        builder.Services.AddSingleton<ITradeCooldownParserFactory, TradeCooldownParserFactory>();

        builder.Services.AddDbContext<SteamTradeApiDbContext>(
            options => options.UseSqlite("Data Source=Database/SteamTradeApiDb.db"));

        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
        builder.AddMapster();

        builder.AddProxyInfrastructure();
        builder.AddSteamClients();


        builder.Services.Configure<ApiBehaviorOptions>(
            options => options.SuppressInferBindingSourcesForParameters = true);
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}

internal static class DependencyInjectionExt
{
    public static void AddMapster(this WebApplicationBuilder builder)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());
        builder.Services.AddSingleton(config);
        builder.Services.AddScoped<IMapper, ServiceMapper>();
    }

    public static void AddProxyInfrastructure(this WebApplicationBuilder builder)
    {
        var config = builder.Configuration;
        var services = builder.Services;

        AddGoodProxiesSource(services, config);
        AddProxyCheckerWithJudges(services);

        services.AddSingleton<ISelfIpAddressProvider, SelfIpAddressProvider>();
        services.AddHostedService<SelfIpAddressUpdaterBackgroundService>();

        services.AddSingleton<IProxyUpdaterService, ProxyUpdaterService>();
        services.AddHostedService<ProxyUpdaterBackgroundService>(); // ToDo

        AddSharedProxyPool(services, config);
        

        static void AddGoodProxiesSource(IServiceCollection services, IConfiguration config)
        {
            services.AddOptions<GoodProxiesRuSettings>()
                .Bind(config.GetSection(nameof(GoodProxiesRuSettings)))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            var apiKey = config.GetValue<string>($"{nameof(GoodProxiesRuSettings)}:ApiKey"); // ToDo: -> getRequiredSec

            services
                .AddRefitClient<IGoodProxiesRuApi>()
                .AddPolicyHandler(HttpPolicyExtensions
                    .HandleTransientHttpError().WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(5)))
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.good-proxies.ru"))
                .AddHttpMessageHandler(() => new AuthQueryApiKeyHandler(apiKey));

            services.AddSingleton<IProxySource, GoodProxiesRuSource>();
        }

        static void AddSharedProxyPool(IServiceCollection services, IConfiguration config)
        {
            var sharedProxyPoolSettings = new ProxyPoolSettings();
            config.Bind($"{nameof(ProxyPoolSettings)}:Shared", sharedProxyPoolSettings);
            services.AddSingleton(new PooledWebProxyProvider(Options.Create(sharedProxyPoolSettings)));

            services.AddSingleton<IProxyUpdateConsumer>(provider =>
                provider.GetRequiredService<PooledWebProxyProvider>());
        }

        void AddProxyCheckerWithJudges(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ProxyChecker>();
            serviceCollection.AddSingleton<IProxyAnonymityJudge, MojeipNetPlAnonymityJudge>();
            serviceCollection.AddSingleton<ProxyAnonymityByHeadersChecker>();
        }
    }

    public static void AddSteamClients(this WebApplicationBuilder builder)
    {
        var config = builder.Configuration;
        var services = builder.Services;

        var generalSteamRefitClientSettings =
            new RefitSettings(new SystemTextJsonContentSerializer(SteamApiJsonSettings.Default));

        AddSteamPricesRemoteService(services, generalSteamRefitClientSettings);
        AddSteamInventoriesRemoteService(services, config, generalSteamRefitClientSettings);


        static void AddSteamPricesRemoteService(IServiceCollection services, RefitSettings refitSettings)
        {
            services.AddSingleton<ISteamPricesRemoteService, SteamPricesRemoteService>();
            services
                .AddRefitClient<ISteamPricesClient>(refitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://steamcommunity.com"))
                .AddPolicyHandler(ISteamPricesClient.SteamPricesRetryPolicy)
                .AddPolicyHandler(ISteamPricesClient.SteamPricesTimeoutPolicy)
                .ConfigurePrimaryHttpMessageHandler(sp => new HttpClientHandler
                {
                    Proxy = sp.GetRequiredService<PooledWebProxyProvider>()
                });
        }

        static void AddSteamInventoriesRemoteService(
            IServiceCollection services,
            IConfiguration config,
            RefitSettings refitSettings)
        {
            // ToDo: move to file
            var proxyPoolWithCredentials = new Dictionary<Uri, NetworkCredential>
            {
                [new Uri("socks5://194.32.229.151:5501")] = new("31254134", "ProxySoxybot"),
                [new Uri("socks5://45.87.253.164:5501")] = new("31254134", "ProxySoxybot"),
                [new Uri("socks5://46.8.23.191:5501")] = new("31254134", "ProxySoxybot"),
                [new Uri("socks5://45.87.253.149:5501")] = new("31254134", "ProxySoxybot"),
                [new Uri("socks5://185.181.246.66:5501")] = new("31254134", "ProxySoxybot"),
                [new Uri("socks5://46.8.110.207:5501")] = new("31254134", "ProxySoxybot"),
                [new Uri("socks5://213.226.101.58:5501")] = new("31254134", "ProxySoxybot"),
                [new Uri("socks5://188.130.136.237:5501")] = new("31254134", "ProxySoxybot"),
                [new Uri("socks5://46.8.22.6:5501")] = new("31254134", "ProxySoxybot"),
                [new Uri("socks5://188.130.143.52:5501")] = new("31254134", "ProxySoxybot"),
                [new Uri("socks5://46.8.222.7:5501")] = new("31254134", "ProxySoxybot"),
                [new Uri("socks5://109.248.204.137:5501")] = new("31254134", "ProxySoxybot"),
            };

            var inventoryProxyPoolSettings = new ProxyPoolSettings();
            config.Bind($"{nameof(ProxyPoolSettings)}:Inventory", inventoryProxyPoolSettings);
            var inventoryProxyPool = PooledWebProxyProvider.CreateWithCredentials(
                proxyPoolWithCredentials, Options.Create(inventoryProxyPoolSettings));

            services.AddSingleton<ISteamInventoriesRemoteService, SteamInventoriesRemoteService>();
            services
                .AddRefitClient<ISteamInventoriesClient>(refitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://steamcommunity.com"))
                .AddPolicyHandler(ISteamInventoriesClient.SteamInventoriesRetryPolicy)
                .AddPolicyHandler(ISteamInventoriesClient.SteamInventoriesTimeoutPolicy)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { Proxy = inventoryProxyPool });
        }
    }
}