using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Refit;
using SteamClientTestPolygonWebApi.Application.Common;
using SteamClientTestPolygonWebApi.Application.Utils.TradeCooldownParsers;
using SteamClientTestPolygonWebApi.Contracts.External;
using SteamClientTestPolygonWebApi.Helpers.Refit;
using SteamClientTestPolygonWebApi.Infrastructure.Persistence;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.BackGroundServices;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker.ProxyAnonymityJudges;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker.ProxySources;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker.ProxySources.GoodProxiesRu;
using SteamClientTestPolygonWebApi.Infrastructure.SteamClients;

namespace SteamClientTestPolygonWebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddDebug();

        builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        builder.Services.AddSingleton<ITradeCooldownParserFactory, TradeCooldownParserFactory>();

        builder.Services.AddDbContext<SteamTradeApiDbContext>(
            options => options.UseSqlite("Data Source=Database/SteamTradeApiDb.db"));

        AddProxyInfrastructure(builder);
        AddSteamClients(builder);

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

    private static void AddProxyInfrastructure(WebApplicationBuilder builder)
    {
        var config = builder.Configuration;
        var services = builder.Services;

        services.AddOptions<GoodProxiesRuSettings>()
            .Bind(config.GetSection(nameof(GoodProxiesRuSettings)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var apiKey = config.GetValue<string>($"{nameof(GoodProxiesRuSettings)}:ApiKey"); // ToDo: -> getRequiredSec
        services.AddRefitClient<IGoodProxiesRuApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.good-proxies.ru"))
            .AddHttpMessageHandler(() => new AuthQueryApiKeyHandler(apiKey));
        services.AddSingleton<IProxySource, GoodProxiesRuSource>();

        services.AddSingleton<ISelfIpAddressProvider, SelfIpAddressProvider>();
        services.AddHostedService<SelfIpAddressUpdaterBackgroundService>();

        services.AddSingleton<IProxyUpdaterService, ProxyUpdaterService>();
        //services.AddHostedService<ProxyUpdaterBackgroundService>(); // ToDo

        var sharedProxyPoolSettings = new ProxyPoolSettings();
        config.Bind($"{nameof(ProxyPoolSettings)}:Shared", sharedProxyPoolSettings);
        services.AddSingleton(new PooledWebProxyProvider(Options.Create(sharedProxyPoolSettings)));

        services.AddSingleton<IProxyUpdateConsumer>(
            provider => provider.GetRequiredService<PooledWebProxyProvider>());

        services.AddSingleton<ProxyChecker>();
        services.AddSingleton<IProxyAnonymityJudge, MojeipNetPlAnonymityJudge>();
        services.AddSingleton<ProxyAnonymityByHeadersChecker>();
    }

    private static void AddSteamClients(WebApplicationBuilder builder)
    {
        var config = builder.Configuration;
        var services = builder.Services;

        var generalSteamRefitClientSettings =
            new RefitSettings(new SystemTextJsonContentSerializer(SteamApiJsonSettings.Default));

        services.AddRefitClient<ISteamPricesClient>(generalSteamRefitClientSettings)
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri("https://steamcommunity.com");
                c.Timeout = TimeSpan.FromSeconds(10);
            })
            .ConfigurePrimaryHttpMessageHandler(sp => new HttpClientHandler
            {
                Proxy = sp.GetRequiredService<PooledWebProxyProvider>()
            });

        // ToDo: move to file
        var proxyPoolWithCredentials = new Dictionary<Uri, NetworkCredential>
        {
            [new Uri("socks5://46.8.22.6:5501")] = new("31254134", "ProxySoxybot"),
            [new Uri("socks5://188.130.143.52:5501")] = new("31254134", "ProxySoxybot"),
            [new Uri("socks5://46.8.222.7:5501")] = new("31254134", "ProxySoxybot"),
            [new Uri("socks5://109.248.204.137:5501")] = new("31254134", "ProxySoxybot"),
            [new Uri("socks5://45.87.253.164:5501")] = new("31254134", "ProxySoxybot"),
            [new Uri("socks5://46.8.23.191:5501")] = new("31254134", "ProxySoxybot"),
            [new Uri("socks5://45.87.253.149:5501")] = new("31254134", "ProxySoxybot"),
            [new Uri("socks5://185.181.246.66:5501")] = new("31254134", "ProxySoxybot"),
            [new Uri("socks5://46.8.110.207:5501")] = new("31254134", "ProxySoxybot"),
            [new Uri("socks5://213.226.101.58:5501")] = new("31254134", "ProxySoxybot"),
            [new Uri("socks5://188.130.136.237:5501")] = new("31254134", "ProxySoxybot"),
        };

        var inventoryProxyPoolSettings = new ProxyPoolSettings();
        config.Bind($"{nameof(ProxyPoolSettings)}:Inventory", inventoryProxyPoolSettings);
        var inventoryProxyPool = PooledWebProxyProvider.CreateWithCredentials(
            proxyPoolWithCredentials, Options.Create(inventoryProxyPoolSettings));

        services.AddRefitClient<ISteamInventoriesClient>(generalSteamRefitClientSettings)
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri("https://steamcommunity.com");
                c.Timeout = TimeSpan.FromSeconds(10);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { Proxy = inventoryProxyPool });
    }

    //services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
}