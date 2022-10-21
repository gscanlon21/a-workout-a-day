using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class ProgressionTable2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_variation_exercise_ExerciseId",
                table: "variation");

            migrationBuilder.DropIndex(
                name: "IX_variation_ExerciseId",
                table: "variation");

            migrationBuilder.DropColumn(
                name: "ExerciseId",
                table: "variation");

            migrationBuilder.DropColumn(
                name: "Progression_Max",
                table: "variation");

            migrationBuilder.DropColumn(
                name: "Progression_Min",
                table: "variation");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExerciseId",
                table: "variation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Progression_Max",
                table: "variation",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Progression_Min",
                table: "variation",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_variation_ExerciseId",
                table: "variation",
                column: "ExerciseId");

            migrationBuilder.AddForeignKey(
                name: "FK_variation_exercise_ExerciseId",
                table: "variation",
                column: "ExerciseId",
                principalTable: "exercise",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
