using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class ProgressionRecord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Progression_Max",
                table: "Intensity",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Progression_Min",
                table: "Intensity",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Progression_Max",
                table: "Intensity");

            migrationBuilder.DropColumn(
                name: "Progression_Min",
                table: "Intensity");
        }
    }
}
