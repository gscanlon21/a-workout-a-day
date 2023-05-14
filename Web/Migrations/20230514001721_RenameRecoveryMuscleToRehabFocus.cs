using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class RenameRecoveryMuscleToRehabFocus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RecoveryMuscle",
                table: "user",
                newName: "RehabFocus");

            migrationBuilder.RenameColumn(
                name: "RecoveryMuscle",
                table: "exercise_variation",
                newName: "RehabFocus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RehabFocus",
                table: "user",
                newName: "RecoveryMuscle");

            migrationBuilder.RenameColumn(
                name: "RehabFocus",
                table: "exercise_variation",
                newName: "RecoveryMuscle");
        }
    }
}
