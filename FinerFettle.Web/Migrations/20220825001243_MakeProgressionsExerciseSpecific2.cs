using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class MakeProgressionsExerciseSpecific2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExerciseUserProgression_ExerciseId",
                table: "ExerciseUserProgression");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseUserProgression_UserId",
                table: "ExerciseUserProgression");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseUserProgression_ExerciseId",
                table: "ExerciseUserProgression",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseUserProgression_UserId",
                table: "ExerciseUserProgression",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExerciseUserProgression_ExerciseId",
                table: "ExerciseUserProgression");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseUserProgression_UserId",
                table: "ExerciseUserProgression");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseUserProgression_ExerciseId",
                table: "ExerciseUserProgression",
                column: "ExerciseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseUserProgression_UserId",
                table: "ExerciseUserProgression",
                column: "UserId",
                unique: true);
        }
    }
}
