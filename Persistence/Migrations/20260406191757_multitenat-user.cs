using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class multitenatuser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "User",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "User",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "User",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "User",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "User",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "User");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "User");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "User");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "User");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "User");
        }
    }
}
