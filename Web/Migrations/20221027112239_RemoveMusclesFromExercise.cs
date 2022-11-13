using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    public partial class RemoveMusclesFromExercise : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRecovery",
                table: "exercise");

            migrationBuilder.RenameColumn(
                name: "Muscles",
                table: "exercise",
                newName: "RecoveryMuscle");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RecoveryMuscle",
                table: "exercise",
                newName: "Muscles");

            migrationBuilder.AddColumn<bool>(
                name: "IsRecovery",
                table: "exercise",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
