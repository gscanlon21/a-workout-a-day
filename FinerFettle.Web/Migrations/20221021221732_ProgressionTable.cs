using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class ProgressionTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exercise_progression",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Progression_Min = table.Column<int>(type: "integer", nullable: true),
                    Progression_Max = table.Column<int>(type: "integer", nullable: true),
                    ExerciseId = table.Column<int>(type: "integer", nullable: false),
                    VariationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_progression", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exercise_progression_exercise_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "exercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exercise_progression_variation_VariationId",
                        column: x => x.VariationId,
                        principalTable: "variation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Variation progressions for an exercise track");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_progression_ExerciseId",
                table: "exercise_progression",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_progression_VariationId",
                table: "exercise_progression",
                column: "VariationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exercise_progression");
        }
    }
}
