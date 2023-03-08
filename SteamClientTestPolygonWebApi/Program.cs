using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Refit;
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

        builder.Services.AddDbContext<SteamTradeApiDbContext>(
            options => options.UseSqlite("Data Source=SteamTradeApiDb.db"));

        AddProxyInfrastructure(builder);
        AddSteamClients(builder);

        builder.Services.AddMemoryCache();
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
        var goodProxiesRuSettings = new GoodProxiesRuSettings();
        builder.Configuration.Bind($"{nameof(GoodProxiesRuSettings)}", goodProxiesRuSettings);
        builder.Services.AddSingleton(Options.Create(goodProxiesRuSettings));

        builder.Services.AddRefitClient<IGoodProxiesRuApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.good-proxies.ru"))
            .AddHttpMessageHandler(() => new AuthQueryApiKeyHandler(goodProxiesRuSettings.ApiKey));
        builder.Services.AddSingleton<IProxySource, GoodProxiesRuSource>();

        builder.Services.AddSingleton<ISelfIpAddressProvider, SelfIpAddressProvider>();
        builder.Services.AddHostedService<SelfIpAddressUpdaterBackgroundService>();

        builder.Services.AddSingleton<IProxyUpdaterService, ProxyUpdaterService>();
        //builder.Services.AddHostedService<ProxyUpdaterBackgroundService>(); // ToDo

        var sharedProxyPoolSettings = new ProxyPoolSettings();
        builder.Configuration.Bind($"{nameof(ProxyPoolSettings)}:Shared", sharedProxyPoolSettings);
        builder.Services.AddSingleton(new PooledWebProxyProvider(Options.Create(sharedProxyPoolSettings)));

        builder.Services.AddSingleton<IProxyUpdateConsumer>(
            provider => provider.GetRequiredService<PooledWebProxyProvider>());

        builder.Services.AddSingleton<ProxyChecker>();
        builder.Services.AddSingleton<IProxyAnonymityJudge, MojeipNetPlAnonymityJudge>();
        builder.Services.AddSingleton<ProxyAnonymityByHeadersChecker>();
    }

    private static void AddSteamClients(WebApplicationBuilder builder)
    {
        var generalSteamRefitClientSettings =
            new RefitSettings(new SystemTextJsonContentSerializer(SteamApiJsonSettings.Default));

        builder.Services.AddRefitClient<ISteamPricesClient>(generalSteamRefitClientSettings)
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri("https://steamcommunity.com");
                c.Timeout = TimeSpan.FromSeconds(10);
            })
            .ConfigurePrimaryHttpMessageHandler(sp => new HttpClientHandler
            {
                Proxy = sp.GetRequiredService<PooledWebProxyProvider>()
            });

        var inventoryProxyPoolSettings = new ProxyPoolSettings();
        builder.Configuration.Bind($"{nameof(ProxyPoolSettings)}:Inventory", inventoryProxyPoolSettings);
        var proxyPoolWithCredentials = new Dictionary<Uri, NetworkCredential>
        {
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
            [new Uri("socks5://109.248.204.137:5501")] = new("31254134", "ProxySoxybot")
        }; // ToDo: move to file
        var inventoryProxyPool = PooledWebProxyProvider.CreateWithCredentials(
            proxyPoolWithCredentials, Options.Create(inventoryProxyPoolSettings));

        builder.Services.AddRefitClient<ISteamInventoriesClient>(generalSteamRefitClientSettings)
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri("https://steamcommunity.com");
                c.Timeout = TimeSpan.FromSeconds(10);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                Proxy = inventoryProxyPool
            });
    }

    //services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
}