using Microsoft.EntityFrameworkCore;
using SteamClientTestPolygonWebApi.Contracts.External;
using SteamClientTestPolygonWebApi.Domain.GameInventoryAggregate;
using SteamClientTestPolygonWebApi.Domain.GameInventoryAggregate.Entities;
using SteamClientTestPolygonWebApi.Domain.GameItemAggregate;

namespace SteamClientTestPolygonWebApi.Infrastructure.Persistence;

public class SteamTradeApiDbContext : DbContext
{
    public SteamTradeApiDbContext(DbContextOptions<SteamTradeApiDbContext> options)
        : base(options) { }

    public DbSet<GameInventory> Inventories { get; set; } = null!;
    
    public DbSet<GameInventoryAsset> Assets { get; set; } = null!;
    
    public DbSet<GameItem> Items { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SteamTradeApiDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}