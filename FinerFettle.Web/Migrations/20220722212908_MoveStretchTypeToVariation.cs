using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class MoveStretchTypeToVariation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProficiencySets",
                table: "Variation",
                newName: "Proficiency_Sets");

            migrationBuilder.RenameColumn(
                name: "ProficiencySecs",
                table: "Variation",
                newName: "Proficiency_Secs");

            migrationBuilder.RenameColumn(
                name: "ProficiencyReps",
                table: "Variation",
                newName: "Proficiency_Reps");

            migrationBuilder.AddColumn<int>(
                name: "Proficiency_Intensity",
                table: "Variation",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Proficiency_Intensity",
                table: "Variation");

            migrationBuilder.RenameColumn(
                name: "Proficiency_Sets",
                table: "Variation",
                newName: "ProficiencySets");

            migrationBuilder.RenameColumn(
                name: "Proficiency_Secs",
                table: "Variation",
                newName: "ProficiencySecs");

            migrationBuilder.RenameColumn(
                name: "Proficiency_Reps",
                table: "Variation",
                newName: "ProficiencyReps");
        }
    }
}
