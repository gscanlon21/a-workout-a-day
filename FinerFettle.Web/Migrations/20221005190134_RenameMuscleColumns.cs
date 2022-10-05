using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class RenameMuscleColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnsafeForMuscles",
                table: "Exercise");

            migrationBuilder.RenameColumn(
                name: "Muscles",
                table: "Exercise",
                newName: "StrengtheningMuscles");

            migrationBuilder.AddColumn<int>(
                name: "StabilizingMuscles",
                table: "Exercise",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StabilizingMuscles",
                table: "Exercise");

            migrationBuilder.RenameColumn(
                name: "StrengtheningMuscles",
                table: "Exercise",
                newName: "Muscles");

            migrationBuilder.AddColumn<int>(
                name: "UnsafeForMuscles",
                table: "Exercise",
                type: "integer",
                nullable: true);
        }
    }
}
