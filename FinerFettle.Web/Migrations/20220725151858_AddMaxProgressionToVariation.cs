using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class AddMaxProgressionToVariation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Progression",
                table: "Variation",
                newName: "MinProgression");

            migrationBuilder.AddColumn<int>(
                name: "MaxProgression",
                table: "Variation",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxProgression",
                table: "Variation");

            migrationBuilder.RenameColumn(
                name: "MinProgression",
                table: "Variation",
                newName: "Progression");
        }
    }
}
