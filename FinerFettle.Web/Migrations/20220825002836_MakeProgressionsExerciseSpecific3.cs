using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class MakeProgressionsExerciseSpecific3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ExerciseUserProgression",
                table: "ExerciseUserProgression");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseUserProgression_ExerciseId",
                table: "ExerciseUserProgression");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ExerciseUserProgression");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExerciseUserProgression",
                table: "ExerciseUserProgression",
                columns: new[] { "ExerciseId", "UserId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ExerciseUserProgression",
                table: "ExerciseUserProgression");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ExerciseUserProgression",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExerciseUserProgression",
                table: "ExerciseUserProgression",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseUserProgression_ExerciseId",
                table: "ExerciseUserProgression",
                column: "ExerciseId");
        }
    }
}
