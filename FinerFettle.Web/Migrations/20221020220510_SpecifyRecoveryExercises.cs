using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class SpecifyRecoveryExercises : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRecovery",
                table: "exercise",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRecovery",
                table: "exercise");
        }
    }
}
