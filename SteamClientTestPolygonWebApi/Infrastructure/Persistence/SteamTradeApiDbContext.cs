using Microsoft.EntityFrameworkCore;
using SteamClientTestPolygonWebApi.Contracts.External;

namespace SteamClientTestPolygonWebApi.Infrastructure.Persistence;

public class SteamTradeApiDbContext : DbContext
{
    public SteamTradeApiDbContext(DbContextOptions<SteamTradeApiDbContext> options)
        : base(options) { }

    public DbSet<SteamSdkInventoryResponse> SteamApiInventoryResponses { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SteamTradeApiDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}