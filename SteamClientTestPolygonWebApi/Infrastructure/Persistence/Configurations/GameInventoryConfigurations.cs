using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamClientTestPolygonWebApi.Domain.GameInventoryAggregate;
using SteamClientTestPolygonWebApi.Domain.GameItemAggregate;

namespace SteamClientTestPolygonWebApi.Infrastructure.Persistence.Configurations;

public class InventoryConfigurations : IEntityTypeConfiguration<GameInventory>
{
    public void Configure(EntityTypeBuilder<GameInventory> builder)
    {
        ConfigureGameInventoryTable(builder);
        ConfigureGameInventoryAssetTable(builder);
    }

    private static void ConfigureGameInventoryTable(EntityTypeBuilder<GameInventory> invBuilder)
    {
        invBuilder.ToTable("GameInventories");

        invBuilder.HasKey(inv => new { inv.OwnerSteam64Id, inv.AppId });

        invBuilder.Property(inv => inv.OwnerSteam64Id)
            .HasMaxLength(100);

        // invBuilder.Property(inv => new { inv.OwnerSteam64Id, inv.AppId })
        //     .ValueGeneratedNever();
    }

    private static void ConfigureGameInventoryAssetTable(EntityTypeBuilder<GameInventory> invBuilder)
    {
        invBuilder.OwnsMany(inv => inv.Assets, assetBuilder =>
        {
            assetBuilder.ToTable("GameInventoryAssets");

            assetBuilder.HasKey(asset => new { asset.AssetId , asset.OwnerSteam64Id, asset.AppId });

            assetBuilder.WithOwner().HasForeignKey(asset => new { asset.OwnerSteam64Id, asset.AppId });
            assetBuilder.HasOne<GameItem>().WithMany()
                .HasForeignKey(asset => new { asset.AppId, asset.ItemMarketHashName })
                .OnDelete(DeleteBehavior.Cascade);

            assetBuilder.Property(asset => asset.AssetId).HasMaxLength(100);

            assetBuilder.Property(asset => asset.ItemMarketHashName).HasMaxLength(100);

            assetBuilder.Property(asset => asset.OwnerSteam64Id).HasMaxLength(100);

            assetBuilder.Property(asset => asset.InstanceId).HasMaxLength(100);
        });

        invBuilder.Metadata.FindNavigation(nameof(GameInventory.Assets))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}