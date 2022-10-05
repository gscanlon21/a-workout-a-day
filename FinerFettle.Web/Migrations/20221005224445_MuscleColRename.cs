using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class MuscleColRename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StrengtheningMuscles",
                table: "Exercise",
                newName: "PrimaryMuscles");

            migrationBuilder.RenameColumn(
                name: "StabilizingMuscles",
                table: "Exercise",
                newName: "SecondaryMuscles");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PrimaryMuscles",
                table: "Exercise",
                newName: "StrengtheningMuscles");

            migrationBuilder.RenameColumn(
                name: "SecondaryMuscles",
                table: "Exercise",
                newName: "StabilizingMuscles");
        }
    }
}
