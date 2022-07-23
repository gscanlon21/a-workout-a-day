using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class MoveStretchIntoIntensityTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Proficiency_Reps",
                table: "Variation");

            migrationBuilder.DropColumn(
                name: "Proficiency_Secs",
                table: "Variation");

            migrationBuilder.DropColumn(
                name: "Proficiency_Sets",
                table: "Variation");

            migrationBuilder.RenameColumn(
                name: "Proficiency_Intensity",
                table: "Variation",
                newName: "IntensityId");

            migrationBuilder.CreateTable(
                name: "Intensity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IntensityLevel = table.Column<int>(type: "integer", nullable: false),
                    Proficiency_Sets = table.Column<int>(type: "integer", nullable: true),
                    Proficiency_Reps = table.Column<int>(type: "integer", nullable: true),
                    Proficiency_Secs = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Intensity", x => x.Id);
                },
                comment: "Intensity level of an exercise variation");

            migrationBuilder.CreateIndex(
                name: "IX_Variation_IntensityId",
                table: "Variation",
                column: "IntensityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Variation_Intensity_IntensityId",
                table: "Variation",
                column: "IntensityId",
                principalTable: "Intensity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Variation_Intensity_IntensityId",
                table: "Variation");

            migrationBuilder.DropTable(
                name: "Intensity");

            migrationBuilder.DropIndex(
                name: "IX_Variation_IntensityId",
                table: "Variation");

            migrationBuilder.RenameColumn(
                name: "IntensityId",
                table: "Variation",
                newName: "Proficiency_Intensity");

            migrationBuilder.AddColumn<int>(
                name: "Proficiency_Reps",
                table: "Variation",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Proficiency_Secs",
                table: "Variation",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Proficiency_Sets",
                table: "Variation",
                type: "integer",
                nullable: true);
        }
    }
}
