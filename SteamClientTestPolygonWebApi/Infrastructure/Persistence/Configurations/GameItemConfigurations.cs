using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamClientTestPolygonWebApi.Domain.GameItemAggregate;

namespace SteamClientTestPolygonWebApi.Infrastructure.Persistence.Configurations;

public class GameItemConfigurations : IEntityTypeConfiguration<GameItem>
{
    public void Configure(EntityTypeBuilder<GameItem> itemBuilder)
    {
        itemBuilder.ToTable("GameItems");
        
        itemBuilder.HasKey(gameItem => new { gameItem.AppId, gameItem.MarketHashName });

        itemBuilder.Property(gameItem => gameItem.MarketHashName).HasMaxLength(100);
        itemBuilder.Property(gameItem => gameItem.IconUrl).HasMaxLength(512);
        itemBuilder.Property(gameItem => gameItem.ClassId).HasMaxLength(100);
    }

    // private static void ConfigureGameInventoryTable(EntityTypeBuilder<GameInventory> invBuilder)
    // {
    //     invBuilder.ToTable("GameInventories");
    //
    //     invBuilder.HasKey(inv => new { inv.OwnerSteam64Id, inv.AppId });
    //
    //     invBuilder.Property(inv => inv.OwnerSteam64Id)
    //         .HasMaxLength(100);
    //
    //     // invBuilder.Property(inv => new { inv.OwnerSteam64Id, inv.AppId })
    //     //     .ValueGeneratedNever();
    // }
}