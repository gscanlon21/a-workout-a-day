using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class BonusType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncludeBonus",
                table: "user");

            migrationBuilder.DropColumn(
                name: "IsBonus",
                table: "exercise_variation");

            migrationBuilder.AddColumn<int>(
                name: "Bonus",
                table: "exercise_variation",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bonus",
                table: "exercise_variation");

            migrationBuilder.AddColumn<bool>(
                name: "IncludeBonus",
                table: "user",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsBonus",
                table: "exercise_variation",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
