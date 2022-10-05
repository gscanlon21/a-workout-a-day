using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class SportsFocusAndRemoveNeedsRest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NeedsRest",
                table: "User");

            migrationBuilder.AddColumn<int>(
                name: "SportsFocus",
                table: "Variation",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RecoveryMuscle",
                table: "User",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SportsFocus",
                table: "User",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SportsFocus",
                table: "Variation");

            migrationBuilder.DropColumn(
                name: "SportsFocus",
                table: "User");

            migrationBuilder.AlterColumn<int>(
                name: "RecoveryMuscle",
                table: "User",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<bool>(
                name: "NeedsRest",
                table: "User",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
