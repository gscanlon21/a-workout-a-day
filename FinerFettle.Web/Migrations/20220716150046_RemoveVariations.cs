using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class RemoveVariations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Variation");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Variation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ExerciseId = table.Column<int>(type: "integer", nullable: true),
                    Instruction = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ProficiencyReps = table.Column<int>(type: "integer", nullable: true),
                    ProficiencySecs = table.Column<int>(type: "integer", nullable: true),
                    ProficiencySets = table.Column<int>(type: "integer", nullable: true),
                    Progression = table.Column<int>(type: "integer", nullable: false),
                    VariationType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Variation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Variation_Exercise_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercise",
                        principalColumn: "Id");
                },
                comment: "Progressions of an exercise");

            migrationBuilder.CreateIndex(
                name: "IX_Variation_ExerciseId",
                table: "Variation",
                column: "ExerciseId");
        }
    }
}
