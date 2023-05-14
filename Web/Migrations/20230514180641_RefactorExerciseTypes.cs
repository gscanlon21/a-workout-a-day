using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class RefactorExerciseTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExerciseFunction",
                table: "exercise_variation");

            migrationBuilder.RenameColumn(
                name: "ExerciseType",
                table: "exercise_variation",
                newName: "ExerciseFocus");

            migrationBuilder.AddColumn<int>(
                name: "ExerciseType",
                table: "variation",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExerciseType",
                table: "variation");

            migrationBuilder.RenameColumn(
                name: "ExerciseFocus",
                table: "exercise_variation",
                newName: "ExerciseType");

            migrationBuilder.AddColumn<int>(
                name: "ExerciseFunction",
                table: "exercise_variation",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
