using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class fieldsstocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StockItemId",
                table: "StockMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "StockItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "StockItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_StockItemId",
                table: "StockMovements",
                column: "StockItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_StockItems_StockItemId",
                table: "StockMovements",
                column: "StockItemId",
                principalTable: "StockItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_StockItems_StockItemId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_StockItemId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "StockItemId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "Comments",
                table: "StockItems");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "StockItems");
        }
    }
}
