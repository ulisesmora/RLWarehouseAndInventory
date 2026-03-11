using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class locationsStorageBin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "physicalOffsetX",
                table: "StorageBins",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "physicalOffsetZ",
                table: "StorageBins",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "rotation",
                table: "StorageBins",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "physicalOffsetX",
                table: "StorageBins");

            migrationBuilder.DropColumn(
                name: "physicalOffsetZ",
                table: "StorageBins");

            migrationBuilder.DropColumn(
                name: "rotation",
                table: "StorageBins");
        }
    }
}
