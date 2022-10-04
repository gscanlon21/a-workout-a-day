using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class IntensityPreference : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntensityPreference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Proficiency_Secs = table.Column<int>(type: "integer", nullable: true),
                    Proficiency_MinReps = table.Column<int>(type: "integer", nullable: true),
                    Proficiency_MaxReps = table.Column<int>(type: "integer", nullable: true),
                    Proficiency_Sets = table.Column<int>(type: "integer", nullable: false),
                    StrengtheningPreference = table.Column<int>(type: "integer", nullable: false),
                    IntensityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntensityPreference", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntensityPreference_Intensity_IntensityId",
                        column: x => x.IntensityId,
                        principalTable: "Intensity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Intensity level of an exercise variation per user's strengthing preference");

            migrationBuilder.CreateIndex(
                name: "IX_IntensityPreference_IntensityId",
                table: "IntensityPreference",
                column: "IntensityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntensityPreference");
        }
    }
}
