using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class MoveProgressionsIntoVariation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Progression",
                table: "Intensity");

            migrationBuilder.AddColumn<int>(
                name: "Progression",
                table: "Variation",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Progression",
                table: "Variation");

            migrationBuilder.AddColumn<int>(
                name: "Progression",
                table: "Intensity",
                type: "integer",
                nullable: true);
        }
    }
}
