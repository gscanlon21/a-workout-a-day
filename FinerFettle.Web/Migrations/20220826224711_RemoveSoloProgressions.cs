using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class RemoveSoloProgressions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxProgression",
                table: "Intensity");

            migrationBuilder.DropColumn(
                name: "MinProgression",
                table: "Intensity");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxProgression",
                table: "Intensity",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinProgression",
                table: "Intensity",
                type: "integer",
                nullable: true);
        }
    }
}
