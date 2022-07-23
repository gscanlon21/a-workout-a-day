using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class ExerciseRotation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MuscleGroups",
                table: "Newsletter");

            migrationBuilder.RenameColumn(
                name: "ExerciseType",
                table: "Newsletter",
                newName: "ExerciseRotationMuscleGroups");

            migrationBuilder.AddColumn<int>(
                name: "ExerciseRotationExerciseType",
                table: "Newsletter",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ExerciseRotaion",
                columns: table => new
                {
                    ExerciseType = table.Column<int>(type: "integer", nullable: false),
                    MuscleGroups = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseRotaion", x => new { x.ExerciseType, x.MuscleGroups });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Newsletter_ExerciseRotationExerciseType_ExerciseRotationMus~",
                table: "Newsletter",
                columns: new[] { "ExerciseRotationExerciseType", "ExerciseRotationMuscleGroups" });

            migrationBuilder.AddForeignKey(
                name: "FK_Newsletter_ExerciseRotaion_ExerciseRotationExerciseType_Exe~",
                table: "Newsletter",
                columns: new[] { "ExerciseRotationExerciseType", "ExerciseRotationMuscleGroups" },
                principalTable: "ExerciseRotaion",
                principalColumns: new[] { "ExerciseType", "MuscleGroups" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Newsletter_ExerciseRotaion_ExerciseRotationExerciseType_Exe~",
                table: "Newsletter");

            migrationBuilder.DropTable(
                name: "ExerciseRotaion");

            migrationBuilder.DropIndex(
                name: "IX_Newsletter_ExerciseRotationExerciseType_ExerciseRotationMus~",
                table: "Newsletter");

            migrationBuilder.DropColumn(
                name: "ExerciseRotationExerciseType",
                table: "Newsletter");

            migrationBuilder.RenameColumn(
                name: "ExerciseRotationMuscleGroups",
                table: "Newsletter",
                newName: "ExerciseType");

            migrationBuilder.AddColumn<int>(
                name: "MuscleGroups",
                table: "Newsletter",
                type: "integer",
                nullable: true);
        }
    }
}
