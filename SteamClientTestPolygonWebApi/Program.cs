using System.Net;
using System.Text.Json;
using Microsoft.OpenApi.Models;
using Refit;
using SteamClientTestPolygonWebApi.ProxyInfrastructure;
using SteamClientTestPolygonWebApi.ProxyInfrastructure.BackGroundServices;
using SteamClientTestPolygonWebApi.ProxyInfrastructure.Checker.ProxyAnonymityJudges;
using SteamClientTestPolygonWebApi.ProxyInfrastructure.Checker.ProxySources;
using SteamClientTestPolygonWebApi.ProxyInfrastructure.Checker.ProxySources.GoodProxiesRu;

namespace SteamClientTestPolygonWebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        AddProxyInfrastructure(builder);

        builder.Services
            .AddHttpClient("SteamClient", client => { client.BaseAddress = new Uri("https://steamcommunity.com"); })
            .ConfigurePrimaryHttpMessageHandler(sp => new HttpClientHandler
            {
                Proxy = sp.GetRequiredService<PooledWebProxyProvider>(),
            });

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
        builder.Services.Configure<GoodProxiesRuSettings>(
            builder.Configuration.GetSection($"{nameof(GoodProxiesRuSettings)}"));
        builder.Services.AddTransient<AuthQueryApiKeyHandler>();
        builder.Services.AddRefitClient<IGoodProxiesRuApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.good-proxies.ru"))
            .AddHttpMessageHandler<AuthQueryApiKeyHandler>();
        builder.Services.AddSingleton<IProxySource, GoodProxiesRuSource>();

        builder.Services.AddSingleton<SelfIpAddressProvider>();
        builder.Services.AddHostedService<SelfIpAddressUpdaterBackgroundService>();

        builder.Services.AddSingleton<IProxyUpdaterService, ProxyUpdaterService>();
        builder.Services.AddHostedService<ProxyUpdaterBackgroundService>();

        builder.Services.Configure<ProxyPoolSettings>(
            builder.Configuration.GetSection($"{nameof(ProxyPoolSettings)}"));
        builder.Services.AddSingleton<PooledWebProxyProvider>();
        builder.Services.AddSingleton<IProxyUpdateConsumer>(
            provider => provider.GetRequiredService<PooledWebProxyProvider>());

        builder.Services.AddSingleton<ProxyChecker>();
        builder.Services.AddSingleton<IProxyAnonymityJudge, MojeipNetPlAnonymityJudge>();
        builder.Services.AddSingleton<ProxyAnonymityByHeadersChecker>();
    }

    //services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
}