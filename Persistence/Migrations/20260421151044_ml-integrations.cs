using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mlintegrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderLines_Materials_MaterialId",
                table: "SalesOrderLines");

            migrationBuilder.AlterColumn<Guid>(
                name: "MaterialId",
                table: "SalesOrderLines",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "ExternalProductName",
                table: "SalesOrderLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalSku",
                table: "SalesOrderLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderLines_Materials_MaterialId",
                table: "SalesOrderLines",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderLines_Materials_MaterialId",
                table: "SalesOrderLines");

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

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderLines_Materials_MaterialId",
                table: "SalesOrderLines",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
