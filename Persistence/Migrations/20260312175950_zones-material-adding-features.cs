using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class zonesmaterialaddingfeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowMixedLots",
                table: "Zones",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AllowedHazmatTags",
                table: "Zones",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "MaxTemperatureCelsius",
                table: "Zones",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinTemperatureCelsius",
                table: "Zones",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HazmatTags",
                table: "Materials",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsFragile",
                table: "Materials",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxStackingLayers",
                table: "Materials",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxTemperatureCelsius",
                table: "Materials",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinTemperatureCelsius",
                table: "Materials",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowMixedLots",
                table: "Zones");

            migrationBuilder.DropColumn(
                name: "AllowedHazmatTags",
                table: "Zones");

            migrationBuilder.DropColumn(
                name: "MaxTemperatureCelsius",
                table: "Zones");

            migrationBuilder.DropColumn(
                name: "MinTemperatureCelsius",
                table: "Zones");

            migrationBuilder.DropColumn(
                name: "HazmatTags",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "IsFragile",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "MaxStackingLayers",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "MaxTemperatureCelsius",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "MinTemperatureCelsius",
                table: "Materials");
        }
    }
}
