using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class RemoveProficiencyFromVariation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Proficiency_MaxReps",
                table: "variation");

            migrationBuilder.DropColumn(
                name: "Proficiency_MinReps",
                table: "variation");

            migrationBuilder.DropColumn(
                name: "Proficiency_Secs",
                table: "variation");

            migrationBuilder.DropColumn(
                name: "Proficiency_Sets",
                table: "variation");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Proficiency_MaxReps",
                table: "variation",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Proficiency_MinReps",
                table: "variation",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Proficiency_Secs",
                table: "variation",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Proficiency_Sets",
                table: "variation",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
