using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class RefreshIntervalTrackPerVariation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAccessoryRefresh",
                table: "newsletter");

            migrationBuilder.DropColumn(
                name: "IsFunctionalRefresh",
                table: "newsletter");

            migrationBuilder.AddColumn<DateOnly>(
                name: "RefreshAfter",
                table: "user_variation",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "RefreshAfter",
                table: "user_exercise_variation",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "RefreshAfter",
                table: "user_exercise",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshAfter",
                table: "user_variation");

            migrationBuilder.DropColumn(
                name: "RefreshAfter",
                table: "user_exercise_variation");

            migrationBuilder.DropColumn(
                name: "RefreshAfter",
                table: "user_exercise");

            migrationBuilder.AddColumn<bool>(
                name: "IsAccessoryRefresh",
                table: "newsletter",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFunctionalRefresh",
                table: "newsletter",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
