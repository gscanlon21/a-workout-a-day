using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class DbComment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "variation",
                comment: "Variations of exercises",
                oldComment: "Intensity level of an exercise variation");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "variation",
                comment: "Intensity level of an exercise variation",
                oldComment: "Variations of exercises");
        }
    }
}
