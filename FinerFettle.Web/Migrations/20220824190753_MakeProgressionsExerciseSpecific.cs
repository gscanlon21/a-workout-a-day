using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class MakeProgressionsExerciseSpecific : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExerciseUserProgression",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ExerciseId = table.Column<int>(type: "integer", nullable: false),
                    Progression = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseUserProgression", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseUserProgression_Exercise_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseUserProgression_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "User's progression level of an exercise");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExerciseUserProgression");
        }
    }
}
