using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class EndMoveProgressionToIntensity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxProgression",
                table: "Variation");

            migrationBuilder.DropColumn(
                name: "MinProgression",
                table: "Variation");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxProgression",
                table: "Variation",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinProgression",
                table: "Variation",
                type: "integer",
                nullable: true);
        }
    }
}
