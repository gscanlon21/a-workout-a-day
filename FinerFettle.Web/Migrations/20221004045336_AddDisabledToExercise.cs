using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class AddDisabledToExercise : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisabledReason",
                table: "Exercise",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisabledReason",
                table: "Exercise");
        }
    }
}
