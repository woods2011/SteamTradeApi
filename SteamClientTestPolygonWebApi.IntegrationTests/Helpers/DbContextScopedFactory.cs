using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SteamClientTestPolygonWebApi.IntegrationTests.Helpers;

public class DbContextScopedFactory<TContext> : IAsyncDisposable where TContext : DbContext
{
    private readonly AsyncServiceScope _scope;
    private readonly Lazy<TContext> _lazyDbContext;
    public TContext DbCtx => _lazyDbContext.Value;

    public DbContextScopedFactory(IServiceProvider serviceProvider)
    {
        _scope = serviceProvider.CreateAsyncScope();
        _lazyDbContext = new Lazy<TContext>(CreateDbContext);
    }

    public DbContextScopedFactory(IServiceScopeFactory serviceScopeFactory)
    {
        _scope = serviceScopeFactory.CreateAsyncScope();
        _lazyDbContext = new Lazy<TContext>(CreateDbContext);
    }

    private TContext CreateDbContext()
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