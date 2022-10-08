using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class ExerciseRequisite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exercise_requisite",
                columns: table => new
                {
                    ExerciseId = table.Column<int>(type: "integer", nullable: false),
                    RequiresProficiencyOfId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_requisite", x => new { x.ExerciseId, x.RequiresProficiencyOfId });
                    table.ForeignKey(
                        name: "FK_exercise_requisite_Exercise_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exercise_requisite_Exercise_RequiresProficiencyOfId",
                        column: x => x.RequiresProficiencyOfId,
                        principalTable: "Exercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Pre-requisite exercises for other exercises");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_requisite_RequiresProficiencyOfId",
                table: "exercise_requisite",
                column: "RequiresProficiencyOfId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exercise_requisite");
        }
    }
}
