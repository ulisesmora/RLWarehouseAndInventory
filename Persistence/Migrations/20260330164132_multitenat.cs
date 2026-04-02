using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class multitenat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SupplierMaterials_SupplierId_MaterialId",
                table: "SupplierMaterials");

            migrationBuilder.DropIndex(
                name: "IX_StockItems_ReferenceNumber",
                table: "StockItems");

            migrationBuilder.DropIndex(
                name: "IX_Materials_SKU",
                table: "Materials");

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Zones",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "WorkOrder",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Warehouses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "UnitOfMeasures",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Suppliers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "SupplierMaterials",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "StorageBins",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "StockMovements",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "StockItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Statuses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "RecipeIngredients",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "RecipeCosts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "ProductRecipes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Materials",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Lots",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Categories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TaxId = table.Column<string>(type: "text", nullable: true),
                    SubscriptionTier = table.Column<string>(type: "text", nullable: false),
                    MaxAllowedLpns = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    RestrictedWarehouseId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierMaterials_SupplierId_MaterialId_OrganizationId",
                table: "SupplierMaterials",
                columns: new[] { "SupplierId", "MaterialId", "OrganizationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_ReferenceNumber_OrganizationId",
                table: "StockItems",
                columns: new[] { "ReferenceNumber", "OrganizationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_SKU_OrganizationId",
                table: "Materials",
                columns: new[] { "SKU", "OrganizationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_OrganizationId",
                table: "User",
                column: "OrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Organization");

            migrationBuilder.DropIndex(
                name: "IX_SupplierMaterials_SupplierId_MaterialId_OrganizationId",
                table: "SupplierMaterials");

            migrationBuilder.DropIndex(
                name: "IX_StockItems_ReferenceNumber_OrganizationId",
                table: "StockItems");

            migrationBuilder.DropIndex(
                name: "IX_Materials_SKU_OrganizationId",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Zones");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "UnitOfMeasures");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "SupplierMaterials");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "StorageBins");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "StockItems");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Statuses");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "RecipeIngredients");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "RecipeCosts");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "ProductRecipes");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Lots");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Categories");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierMaterials_SupplierId_MaterialId",
                table: "SupplierMaterials",
                columns: new[] { "SupplierId", "MaterialId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_ReferenceNumber",
                table: "StockItems",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_SKU",
                table: "Materials",
                column: "SKU",
                unique: true);
        }
    }
}
