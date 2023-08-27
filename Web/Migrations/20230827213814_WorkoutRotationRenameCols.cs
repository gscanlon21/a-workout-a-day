using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class WorkoutRotationRenameCols : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WorkoutRotation_MuscleGroups",
                table: "user_workout",
                newName: "Rotation_MuscleGroups");

            migrationBuilder.RenameColumn(
                name: "WorkoutRotation_MovementPatterns",
                table: "user_workout",
                newName: "Rotation_MovementPatterns");

            migrationBuilder.RenameColumn(
                name: "WorkoutRotation_Id",
                table: "user_workout",
                newName: "Rotation_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Rotation_MuscleGroups",
                table: "user_workout",
                newName: "WorkoutRotation_MuscleGroups");

            migrationBuilder.RenameColumn(
                name: "Rotation_MovementPatterns",
                table: "user_workout",
                newName: "WorkoutRotation_MovementPatterns");

            migrationBuilder.RenameColumn(
                name: "Rotation_Id",
                table: "user_workout",
                newName: "WorkoutRotation_Id");
        }
    }
}
