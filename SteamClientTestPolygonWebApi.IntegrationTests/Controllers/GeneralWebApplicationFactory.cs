using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Refit;
using SteamClientTestPolygonWebApi.Contracts.External;
using SteamClientTestPolygonWebApi.Infrastructure.Persistence;
using SteamClientTestPolygonWebApi.Infrastructure.SteamClients;

namespace SteamClientTestPolygonWebApi.IntegrationTests.Controllers;

public class GeneralWebApplicationFactory : WebApplicationFactory<SteamClientTestPolygonWebApi.Program>
{
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

            using var scope = services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DbContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        });

        builder.UseEnvironment("Development"); // ToDo: check if it's needed
    }
}