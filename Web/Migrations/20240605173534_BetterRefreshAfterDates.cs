using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class BetterRefreshAfterDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "user_exercise");

            migrationBuilder.DropColumn(
                name: "RefreshAfter",
                table: "user_exercise");

            migrationBuilder.DropColumn(
                name: "DeloadAfterEveryXWeeks",
                table: "user");

            migrationBuilder.DropColumn(
                name: "RefreshAccessoryEveryXWeeks",
                table: "user");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "exercise");

            migrationBuilder.RenameColumn(
                name: "RefreshFunctionalEveryXWeeks",
                table: "user",
                newName: "DeloadAfterXWeeks");

            migrationBuilder.AddColumn<DateOnly>(
                name: "RefreshAfter",
                table: "user_variation",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RefreshEveryXWeeks",
                table: "user_variation",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshAfter",
                table: "user_variation");

            migrationBuilder.DropColumn(
                name: "RefreshEveryXWeeks",
                table: "user_variation");

            migrationBuilder.RenameColumn(
                name: "DeloadAfterXWeeks",
                table: "user",
                newName: "RefreshFunctionalEveryXWeeks");

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "user_exercise",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "RefreshAfter",
                table: "user_exercise",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeloadAfterEveryXWeeks",
                table: "user",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RefreshAccessoryEveryXWeeks",
                table: "user",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "exercise",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
