using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class DisplayNameForIntensity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Exercise");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Intensity",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Intensity");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Exercise",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
