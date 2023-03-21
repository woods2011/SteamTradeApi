using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SteamClientTestPolygonWebApi.Infrastructure.Persistence;

namespace SteamClientTestPolygonWebApi.IntegrationTests.Controllers;

public class GeneralWebAppFactory : WebApplicationFactory<SteamClientTestPolygonWebApi.Program>, IAsyncLifetime
{
    public HttpClient Client { get; private set; } = null!;
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddControllers().AddControllersAsServices();

            services.RemoveAll(typeof(DbContextOptions<SteamTradeApiDbContext>));
            //services.RemoveAll(typeof(IDbConnectionFactory)); // ToDo: check if it's needed
            //services.RemoveAll(typeof(DbConnection)); // ToDo: check if it's needed

            services.AddSingleton<DbConnection>(_ =>
            {
                var connection = new SqliteConnection("DataSource=:memory:");
                connection.Open();
                return connection;
            });
            
            services.AddDbContext<SteamTradeApiDbContext>((serviceProvider, options) =>
            {
                options.EnableSensitiveDataLogging();
                options.UseSqlite(connection: serviceProvider.GetRequiredService<DbConnection>());
            });

            // Not Working почему-то
            // using var scope = services.BuildServiceProvider().CreateScope();
            // using var context = scope.ServiceProvider.GetRequiredService<SteamTradeApiDbContext>();
            // context.Database.EnsureDeleted();
            // context.Database.EnsureCreated();
        });

        builder.UseEnvironment("Development"); // ToDo: check if it's needed
    }

    public async Task InitializeAsync()
    {
        Client = CreateClient();
        using var serviceScope = Services.CreateScope();
        var dbCtx = serviceScope.ServiceProvider.GetRequiredService<SteamTradeApiDbContext>();
        await dbCtx.Database.EnsureDeletedAsync();
        await dbCtx.Database.EnsureCreatedAsync();
    }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
}

// await using var dbContextFactory = _factory.CreateDbContextFactory();
// var dbContext = dbContextFactory.DbCtx;