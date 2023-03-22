using AutoFixture;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using RichardSzalay.MockHttp;
using SteamClientTestPolygonWebApi.Application.SteamRemoteServices;
using SteamClientTestPolygonWebApi.Contracts.External;
using SteamClientTestPolygonWebApi.Infrastructure.Persistence;
using SteamClientTestPolygonWebApi.Infrastructure.SteamRefitClients;
using SteamClientTestPolygonWebApi.IntegrationTests.Helpers;

namespace SteamClientTestPolygonWebApi.IntegrationTests.Controllers.Inventory.Setup;

[CollectionDefinition(nameof(InventoryWebAppFactoryCollection))]
public class InventoryWebAppFactoryCollection : ICollectionFixture<InventoryWebAppFactory> { }

public class InventoryWebAppFactory : GeneralWebAppFactory
{
    public Fixture Fixture { get; } = new();
    public MockHttpMessageHandler MockHttp { get; } = new();

    public string SerializedSteamSdkInventoryResponseExample { get; } =
        File.ReadAllText($"{ExternalApisResponsesJsonRootPath}/SteamSdkInventoryResponseExample.json");

    public string SerializedSteamSdkItemPriceResponseExample { get; } =
        File.ReadAllText($"{ExternalApisResponsesJsonRootPath}/SteamSdkItemPriceResponseExample.json");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        var generalSteamRefitClientSettings =
            new RefitSettings(new SystemTextJsonContentSerializer(SteamApiJsonSettings.Default));

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<ISteamPricesRemoteService, SteamPricesRemoteService>();
            services.AddRefitClient<ISteamPricesClient>(generalSteamRefitClientSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://steamcommunity.com"))
                .ConfigureHttpMessageHandlerBuilder(b =>
                {
                    b.PrimaryHandler = MockHttp;
                    b.AdditionalHandlers.Clear();
                });

            services.AddSingleton<ISteamInventoriesRemoteService, OfficialSteamInventoriesService>();
            services.AddRefitClient<IOfficialSteamInventoriesClient>(generalSteamRefitClientSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://steamcommunity.com"))
                .ConfigureHttpMessageHandlerBuilder(b =>
                {
                    b.PrimaryHandler = MockHttp;
                    b.AdditionalHandlers.Clear();
                });
        });
    }

    public DbContextScopedFactory<SteamTradeApiDbContext> CreateDbContextFactory() => new(Services);

    private static readonly string ExternalApisResponsesJsonRootPath =
        $"{Directory.GetCurrentDirectory()}/ExternalApisResponsesJson";
}

// using System.Net;
// using Microsoft.AspNetCore.Hosting;
// using Microsoft.AspNetCore.TestHost;
// using Microsoft.Extensions.DependencyInjection;
// using Refit;
// using RichardSzalay.MockHttp;
// using SteamClientTestPolygonWebApi.Application.SteamRemoteServices;
// using SteamClientTestPolygonWebApi.Contracts.External;
// using SteamClientTestPolygonWebApi.Infrastructure.Persistence;
//
// namespace SteamClientTestPolygonWebApi.IntegrationTests.Controllers.Inventory.Setup;
//
// [CollectionDefinition(nameof(InventoryControllerCollection))]
// public class InventoryControllerCollection : ICollectionFixture<InventoryControllerFixture> { }
//
// public class InventoryControllerFixture
// {
//     public InventoryWebApplicationFactory WebAppFactory { get; } = new();
//
//     public InventoryControllerFixture()
//     {
//         using var serviceScope = WebAppFactory.Services.CreateScope();
//         var dbCtx = serviceScope.ServiceProvider.GetRequiredService<SteamTradeApiDbContext>();
//         dbCtx.Database.EnsureDeletedAsync();
//         dbCtx.Database.EnsureCreatedAsync();
//     }
//
//     public string SerializedSteamSdkInventoryResponseExample { get; } =
//         File.ReadAllText($"{ExternalApisResponsesJsonPath}/SteamSdkInventoryResponseExample.json");
//
//     private static string ExternalApisResponsesJsonPath =>
//         $"{Directory.GetCurrentDirectory()}/Controllers/Inventory/ExternalApisResponsesJson";
// }
//
// public class InventoryWebApplicationFactory : GeneralWebApplicationFactory
// {
//     public MockHttpMessageHandler MockHttp { get; } = new();
//
//     protected override void ConfigureWebHost(IWebHostBuilder builder)
//     {
//         base.ConfigureWebHost(builder);
//
//         var generalSteamRefitClientSettings =
//             new RefitSettings(new SystemTextJsonContentSerializer(SteamApiJsonSettings.Default));
//
//         builder.ConfigureTestServices(services =>
//         {
//             services.AddRefitClient<ISteamInventoriesClient>(generalSteamRefitClientSettings)
//                 .ConfigureHttpClient(c =>
//                 {
//                     c.BaseAddress = new Uri("https://steamcommunity.com");
//                     c.Timeout = TimeSpan.FromSeconds(10);
//                 })
//                 // .AddPolicyHandler(ISteamInventoriesClient.SteamInventoriesRetryPolicy)
//                 // .AddPolicyHandler(ISteamInventoriesClient.SteamInventoriesTimeoutPolicy)
//                 .ConfigurePrimaryHttpMessageHandler(() => MockHttp);
//         });
//     }
// }