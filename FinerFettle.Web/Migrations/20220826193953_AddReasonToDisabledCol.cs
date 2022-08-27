using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class AddReasonToDisabledCol : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Enabled",
                table: "Variation");

            migrationBuilder.AddColumn<string>(
                name: "DisabledReason",
                table: "Variation",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisabledReason",
                table: "Variation");

            migrationBuilder.AddColumn<bool>(
                name: "Enabled",
                table: "Variation",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
