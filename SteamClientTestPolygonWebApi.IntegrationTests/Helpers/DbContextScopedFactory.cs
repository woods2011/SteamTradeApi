using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SteamClientTestPolygonWebApi.IntegrationTests.Helpers;

public class DbContextScopedFactory<TContext> : IAsyncDisposable where TContext : DbContext
{
    private readonly AsyncServiceScope _scope;

    public DbContextScopedFactory(IServiceProvider serviceProvider) =>
        _scope = serviceProvider.CreateAsyncScope();

    public DbContextScopedFactory(IServiceScopeFactory serviceScopeFactory) =>
        _scope = serviceScopeFactory.CreateAsyncScope();

    public TContext CreateDbContext()
    {
        var dbContext = _scope.ServiceProvider.GetRequiredService<TContext>();
        return dbContext;
    }

    public async ValueTask DisposeAsync()
    {
        await _scope.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}