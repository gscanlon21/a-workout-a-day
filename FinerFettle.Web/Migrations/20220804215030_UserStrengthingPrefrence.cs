using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class UserStrengthingPrefrence : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MobilityMuscles",
                table: "User");

            migrationBuilder.DropColumn(
                name: "PrefersEccentricExercises",
                table: "User");

            migrationBuilder.DropColumn(
                name: "PrefersWeightedExercises",
                table: "User");

            migrationBuilder.DropColumn(
                name: "RecoveryMuscles",
                table: "User");

            migrationBuilder.RenameColumn(
                name: "StrengthMuscles",
                table: "User",
                newName: "StrengtheningPreference");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StrengtheningPreference",
                table: "User",
                newName: "StrengthMuscles");

            migrationBuilder.AddColumn<int>(
                name: "MobilityMuscles",
                table: "User",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "PrefersEccentricExercises",
                table: "User",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PrefersWeightedExercises",
                table: "User",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RecoveryMuscles",
                table: "User",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
