using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addingsizesinmaterialstockitems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContainerType",
                table: "StockItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "HeightCm",
                table: "StockItems",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsStackable",
                table: "StockItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LengthCm",
                table: "StockItems",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightKg",
                table: "StockItems",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WidthCm",
                table: "StockItems",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HeightCm",
                table: "Materials",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LengthCm",
                table: "Materials",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightKg",
                table: "Materials",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WidthCm",
                table: "Materials",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContainerType",
                table: "StockItems");

            migrationBuilder.DropColumn(
                name: "HeightCm",
                table: "StockItems");

            migrationBuilder.DropColumn(
                name: "IsStackable",
                table: "StockItems");

            migrationBuilder.DropColumn(
                name: "LengthCm",
                table: "StockItems");

            migrationBuilder.DropColumn(
                name: "WeightKg",
                table: "StockItems");

            migrationBuilder.DropColumn(
                name: "WidthCm",
                table: "StockItems");

            migrationBuilder.DropColumn(
                name: "HeightCm",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "LengthCm",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "WeightKg",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "WidthCm",
                table: "Materials");
        }
    }
}
