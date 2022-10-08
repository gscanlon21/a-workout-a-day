using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class RefactorIntensity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntensityLevel",
                table: "Intensity");

            migrationBuilder.RenameColumn(
                name: "StrengtheningPreference",
                table: "IntensityPreference",
                newName: "IntensityLevel");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IntensityLevel",
                table: "IntensityPreference",
                newName: "StrengtheningPreference");

            migrationBuilder.AddColumn<int>(
                name: "IntensityLevel",
                table: "Intensity",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
