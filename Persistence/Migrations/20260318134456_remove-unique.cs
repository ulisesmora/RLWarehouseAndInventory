using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class removeunique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockItems_WarehouseId_MaterialId_LotId_StatusId",
                table: "StockItems");

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_ReferenceNumber",
                table: "StockItems",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_WarehouseId_MaterialId_LotId_StatusId",
                table: "StockItems",
                columns: new[] { "WarehouseId", "MaterialId", "LotId", "StatusId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockItems_ReferenceNumber",
                table: "StockItems");

            migrationBuilder.DropIndex(
                name: "IX_StockItems_WarehouseId_MaterialId_LotId_StatusId",
                table: "StockItems");

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_WarehouseId_MaterialId_LotId_StatusId",
                table: "StockItems",
                columns: new[] { "WarehouseId", "MaterialId", "LotId", "StatusId" },
                unique: true);
        }
    }
}
