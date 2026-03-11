using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorkOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StorageBinId",
                table: "StockMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkOrder",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNumber = table.Column<string>(type: "text", nullable: false),
                    ProductRecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinishedGoodId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlannedQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    ProducedQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    PlannedStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualCompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrder_Materials_FinishedGoodId",
                        column: x => x.FinishedGoodId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrder_ProductRecipes_ProductRecipeId",
                        column: x => x.ProductRecipeId,
                        principalTable: "ProductRecipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_StorageBinId",
                table: "StockMovements",
                column: "StorageBinId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_FinishedGoodId",
                table: "WorkOrder",
                column: "FinishedGoodId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_ProductRecipeId",
                table: "WorkOrder",
                column: "ProductRecipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_StorageBins_StorageBinId",
                table: "StockMovements",
                column: "StorageBinId",
                principalTable: "StorageBins",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_StorageBins_StorageBinId",
                table: "StockMovements");

            migrationBuilder.DropTable(
                name: "WorkOrder");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_StorageBinId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "StorageBinId",
                table: "StockMovements");
        }
    }
}
