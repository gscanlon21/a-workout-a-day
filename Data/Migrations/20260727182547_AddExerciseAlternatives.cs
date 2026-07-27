using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseAlternatives : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exercise_alternative",
                columns: table => new
                {
                    ExerciseId = table.Column<int>(type: "integer", nullable: false),
                    AlternativeExerciseId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_alternative", x => new { x.ExerciseId, x.AlternativeExerciseId });
                    table.ForeignKey(
                        name: "FK_exercise_alternative_exercise_AlternativeExerciseId",
                        column: x => x.AlternativeExerciseId,
                        principalTable: "exercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exercise_alternative_exercise_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "exercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exercise_alternative_AlternativeExerciseId",
                table: "exercise_alternative",
                column: "AlternativeExerciseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exercise_alternative");
        }
    }
}
