using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SwitchFromRefreshDaysToWeeks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RefreshFunctionalEveryXDays",
                table: "user",
                newName: "RefreshFunctionalEveryXWeeks");

            migrationBuilder.RenameColumn(
                name: "RefreshAccessoryEveryXDays",
                table: "user",
                newName: "RefreshAccessoryEveryXWeeks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RefreshFunctionalEveryXWeeks",
                table: "user",
                newName: "RefreshFunctionalEveryXDays");

            migrationBuilder.RenameColumn(
                name: "RefreshAccessoryEveryXWeeks",
                table: "user",
                newName: "RefreshAccessoryEveryXDays");
        }
    }
}
