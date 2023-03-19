using AutoFixture;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using RichardSzalay.MockHttp;
using SteamClientTestPolygonWebApi.Application.SteamRemoteServices;
using SteamClientTestPolygonWebApi.Contracts.External;
using SteamClientTestPolygonWebApi.Infrastructure.Persistence;
using SteamClientTestPolygonWebApi.IntegrationTests.Helpers;

namespace SteamClientTestPolygonWebApi.IntegrationTests.Controllers.Inventory.Setup;

[CollectionDefinition(nameof(InventoryWebAppFactoryCollection))]
public class InventoryWebAppFactoryCollection : ICollectionFixture<InventoryControllerWebApplicationFactory> { }

public class InventoryControllerWebApplicationFactory : GeneralWebApplicationFactory
{
    public Fixture Fixture { get; } = new();
    public MockHttpMessageHandler MockHttp { get; } = new();

    public string SerializedSteamSdkInventoryResponseExample { get; } =
        File.ReadAllText($"{ExternalApisResponsesJsonPath}/SteamSdkInventoryResponseExample.json");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        var generalSteamRefitClientSettings =
            new RefitSettings(new SystemTextJsonContentSerializer(SteamApiJsonSettings.Default));

        builder.ConfigureTestServices(services =>
        {
            services.AddRefitClient<ISteamInventoriesClient>(generalSteamRefitClientSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://steamcommunity.com"))
                .ConfigurePrimaryHttpMessageHandler(() => MockHttp);
        });
    }

    public DbContextScopedFactory<SteamTradeApiDbContext> CreateDbContextFactory() => new(Services);

    private static string ExternalApisResponsesJsonPath =>
        $"{Directory.GetCurrentDirectory()}/Controllers/Inventory/ExternalApisResponsesJson";
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