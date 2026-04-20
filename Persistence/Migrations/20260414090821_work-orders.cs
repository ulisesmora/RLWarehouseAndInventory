using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class workorders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductRecipes_Materials_FinishedGoodId",
                table: "ProductRecipes");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeIngredients_Materials_MaterialId",
                table: "RecipeIngredients");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrder_Materials_FinishedGoodId",
                table: "WorkOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrder_ProductRecipes_ProductRecipeId",
                table: "WorkOrder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkOrder",
                table: "WorkOrder");

            migrationBuilder.RenameTable(
                name: "WorkOrder",
                newName: "WorkOrders");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrder_ProductRecipeId",
                table: "WorkOrders",
                newName: "IX_WorkOrders_ProductRecipeId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrder_FinishedGoodId",
                table: "WorkOrders",
                newName: "IX_WorkOrders_FinishedGoodId");

            migrationBuilder.AddColumn<decimal>(
                name: "AllocatedQuantity",
                table: "StockItems",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "QuantityRequired",
                table: "RecipeIngredients",
                type: "numeric(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "EstimatedCost",
                table: "RecipeCosts",
                type: "numeric(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "RecipeCosts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ProductRecipes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "EstimatedMachineHours",
                table: "ProductRecipes",
                type: "numeric(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "EstimatedLaborHours",
                table: "ProductRecipes",
                type: "numeric(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "ProducedQuantity",
                table: "WorkOrders",
                type: "numeric(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "OrderNumber",
                table: "WorkOrders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkOrders",
                table: "WorkOrders",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ProductionPickTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceStockItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequiredQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    PickedQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionPickTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionPickTasks_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionPickTasks_StockItems_SourceStockItemId",
                        column: x => x.SourceStockItemId,
                        principalTable: "StockItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionPickTasks_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderConsumption",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    LotId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceStorageBinId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlannedQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    ActualConsumedQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderConsumption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderConsumption_Lots_LotId",
                        column: x => x.LotId,
                        principalTable: "Lots",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkOrderConsumption_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrderConsumption_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPickTasks_MaterialId",
                table: "ProductionPickTasks",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPickTasks_SourceStockItemId",
                table: "ProductionPickTasks",
                column: "SourceStockItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPickTasks_WorkOrderId",
                table: "ProductionPickTasks",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderConsumption_LotId",
                table: "WorkOrderConsumption",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderConsumption_MaterialId",
                table: "WorkOrderConsumption",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderConsumption_WorkOrderId",
                table: "WorkOrderConsumption",
                column: "WorkOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductRecipes_Materials_FinishedGoodId",
                table: "ProductRecipes",
                column: "FinishedGoodId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeIngredients_Materials_MaterialId",
                table: "RecipeIngredients",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_Materials_FinishedGoodId",
                table: "WorkOrders",
                column: "FinishedGoodId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_ProductRecipes_ProductRecipeId",
                table: "WorkOrders",
                column: "ProductRecipeId",
                principalTable: "ProductRecipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductRecipes_Materials_FinishedGoodId",
                table: "ProductRecipes");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeIngredients_Materials_MaterialId",
                table: "RecipeIngredients");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_Materials_FinishedGoodId",
                table: "WorkOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_ProductRecipes_ProductRecipeId",
                table: "WorkOrders");

            migrationBuilder.DropTable(
                name: "ProductionPickTasks");

            migrationBuilder.DropTable(
                name: "WorkOrderConsumption");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkOrders",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "AllocatedQuantity",
                table: "StockItems");

            migrationBuilder.RenameTable(
                name: "WorkOrders",
                newName: "WorkOrder");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrders_ProductRecipeId",
                table: "WorkOrder",
                newName: "IX_WorkOrder_ProductRecipeId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrders_FinishedGoodId",
                table: "WorkOrder",
                newName: "IX_WorkOrder_FinishedGoodId");

            migrationBuilder.AlterColumn<decimal>(
                name: "QuantityRequired",
                table: "RecipeIngredients",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "EstimatedCost",
                table: "RecipeCosts",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "RecipeCosts",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ProductRecipes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<decimal>(
                name: "EstimatedMachineHours",
                table: "ProductRecipes",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "EstimatedLaborHours",
                table: "ProductRecipes",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ProducedQuantity",
                table: "WorkOrder",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)");

            migrationBuilder.AlterColumn<string>(
                name: "OrderNumber",
                table: "WorkOrder",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkOrder",
                table: "WorkOrder",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductRecipes_Materials_FinishedGoodId",
                table: "ProductRecipes",
                column: "FinishedGoodId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeIngredients_Materials_MaterialId",
                table: "RecipeIngredients",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrder_Materials_FinishedGoodId",
                table: "WorkOrder",
                column: "FinishedGoodId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrder_ProductRecipes_ProductRecipeId",
                table: "WorkOrder",
                column: "ProductRecipeId",
                principalTable: "ProductRecipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
