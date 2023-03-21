using EFCore.BulkExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SteamClientTestPolygonWebApi.Domain.GameInventoryAggregate;
using SteamClientTestPolygonWebApi.Domain.GameInventoryAggregate.Entities;
using SteamClientTestPolygonWebApi.Domain.Item;

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
        modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetProperties())
            .Where(p => p.IsPrimaryKey())
            .ToList()
            .ForEach(p => p.ValueGenerated = ValueGenerated.Never);

        base.OnModelCreating(modelBuilder);
    }
}

public static class DbContextExtensions
{
    public static Task BulkInsertIfNotExistsAsync<T>(
        this DbContext context,
        IList<T> entities,
        CancellationToken ct) where T : class
    {
        return context.BulkInsertOrUpdateAsync(entities,
            config =>
            {
                config.SqlBulkCopyOptions = SqlBulkCopyOptions.CheckConstraints;
                // config.UpdateByProperties = new List<string> { nameof(GameItem.AppId), nameof(GameItem.MarketHashName) };
                config.PropertiesToIncludeOnUpdate = new List<string> { "" };
            },
            cancellationToken: ct);
    }
}