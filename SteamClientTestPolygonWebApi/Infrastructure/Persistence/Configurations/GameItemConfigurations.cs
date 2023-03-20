using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamClientTestPolygonWebApi.Domain.Item;

namespace SteamClientTestPolygonWebApi.Infrastructure.Persistence.Configurations;

public class GameItemConfigurations : IEntityTypeConfiguration<GameItem>
{
    public void Configure(EntityTypeBuilder<GameItem> itemBuilder)
    {
        itemBuilder.ToTable("GameItems");

        itemBuilder.HasKey(gameItem => new { gameItem.AppId, gameItem.MarketHashName });

        itemBuilder.OwnsOne(m => m.PriceInfo);

        itemBuilder.Property(gameItem => gameItem.MarketHashName).HasMaxLength(100);
        itemBuilder.Property(gameItem => gameItem.IconUrl).HasMaxLength(512);
        itemBuilder.Property(gameItem => gameItem.ClassId).HasMaxLength(100);
    }
}