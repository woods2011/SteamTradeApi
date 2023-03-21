using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamClientTestPolygonWebApi.Migrations
{
    public partial class AddItemsPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PriceInfo_LastUpdateUtc",
                table: "GameItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceInfo_LowestMarketPriceUsd",
                table: "GameItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceInfo_MedianMarketPriceUsd",
                table: "GameItems",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceInfo_LastUpdateUtc",
                table: "GameItems");

            migrationBuilder.DropColumn(
                name: "PriceInfo_LowestMarketPriceUsd",
                table: "GameItems");

            migrationBuilder.DropColumn(
                name: "PriceInfo_MedianMarketPriceUsd",
                table: "GameItems");
        }
    }
}
