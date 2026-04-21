using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class salesorderlineexternalproduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hacer MaterialId nullable en SalesOrderLines
            migrationBuilder.AlterColumn<Guid>(
                name: "MaterialId",
                table: "SalesOrderLines",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: false);

            // Agregar columna para el nombre del producto externo
            migrationBuilder.AddColumn<string>(
                name: "ExternalProductName",
                table: "SalesOrderLines",
                type: "text",
                nullable: true);

            // Agregar columna para el SKU externo
            migrationBuilder.AddColumn<string>(
                name: "ExternalSku",
                table: "SalesOrderLines",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalProductName",
                table: "SalesOrderLines");

            migrationBuilder.DropColumn(
                name: "ExternalSku",
                table: "SalesOrderLines");

            migrationBuilder.AlterColumn<Guid>(
                name: "MaterialId",
                table: "SalesOrderLines",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
