using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class MoveProgressionsIntoIntensity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MuscleContractions",
                table: "Variation");

            migrationBuilder.DropColumn(
                name: "Progression",
                table: "Variation");

            migrationBuilder.AddColumn<int>(
                name: "MuscleContractions",
                table: "Intensity",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Progression",
                table: "Intensity",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MuscleContractions",
                table: "Intensity");

            migrationBuilder.DropColumn(
                name: "Progression",
                table: "Intensity");

            migrationBuilder.AddColumn<int>(
                name: "MuscleContractions",
                table: "Variation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Progression",
                table: "Variation",
                type: "integer",
                nullable: true);
        }
    }
}
