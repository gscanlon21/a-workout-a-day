using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class ProficiencyRefactor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Proficiency_Reps",
                table: "Intensity",
                newName: "Proficiency_MinReps");

            migrationBuilder.AddColumn<int>(
                name: "Proficiency_MaxReps",
                table: "Intensity",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Proficiency_MaxReps",
                table: "Intensity");

            migrationBuilder.RenameColumn(
                name: "Proficiency_MinReps",
                table: "Intensity",
                newName: "Proficiency_Reps");
        }
    }
}
