using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class FormName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Instruction",
                table: "Variation");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Form",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Form");

            migrationBuilder.AddColumn<string>(
                name: "Instruction",
                table: "Variation",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
