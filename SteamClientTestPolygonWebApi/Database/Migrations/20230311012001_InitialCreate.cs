using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamClientTestPolygonWebApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameInventories",
                columns: table => new
                {
                    AppId = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnerSteam64Id = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastUpdateTimeUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameInventories", x => new { x.OwnerSteam64Id, x.AppId });
                });

            migrationBuilder.CreateTable(
                name: "GameItems",
                columns: table => new
                {
                    AppId = table.Column<int>(type: "INTEGER", nullable: false),
                    MarketHashName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IconUrl = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    ClassId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameItems", x => new { x.AppId, x.MarketHashName });
                });

            migrationBuilder.CreateTable(
                name: "GameInventoryAssets",
                columns: table => new
                {
                    AssetId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    AppId = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnerSteam64Id = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ItemMarketHashName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsTradable = table.Column<bool>(type: "INTEGER", nullable: false),
                    TradeCooldownUntilUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsMarketable = table.Column<bool>(type: "INTEGER", nullable: false),
                    InstanceId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameInventoryAssets", x => new { x.AssetId, x.OwnerSteam64Id, x.AppId });
                    table.ForeignKey(
                        name: "FK_GameInventoryAssets_GameInventories_OwnerSteam64Id_AppId",
                        columns: x => new { x.OwnerSteam64Id, x.AppId },
                        principalTable: "GameInventories",
                        principalColumns: new[] { "OwnerSteam64Id", "AppId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameInventoryAssets_GameItems_AppId_ItemMarketHashName",
                        columns: x => new { x.AppId, x.ItemMarketHashName },
                        principalTable: "GameItems",
                        principalColumns: new[] { "AppId", "MarketHashName" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameInventoryAssets_AppId_ItemMarketHashName",
                table: "GameInventoryAssets",
                columns: new[] { "AppId", "ItemMarketHashName" });

            migrationBuilder.CreateIndex(
                name: "IX_GameInventoryAssets_OwnerSteam64Id_AppId",
                table: "GameInventoryAssets",
                columns: new[] { "OwnerSteam64Id", "AppId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameInventoryAssets");

            migrationBuilder.DropTable(
                name: "GameInventories");

            migrationBuilder.DropTable(
                name: "GameItems");
        }
    }
}
