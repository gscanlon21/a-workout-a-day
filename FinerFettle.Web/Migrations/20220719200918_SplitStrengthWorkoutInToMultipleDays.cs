using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class SplitStrengthWorkoutInToMultipleDays : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WantsStrength",
                table: "User");

            migrationBuilder.RenameColumn(
                name: "Equipment",
                table: "Newsletter",
                newName: "MuscleGroups");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MuscleGroups",
                table: "Newsletter",
                newName: "Equipment");

            migrationBuilder.AddColumn<bool>(
                name: "WantsStrength",
                table: "User",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
