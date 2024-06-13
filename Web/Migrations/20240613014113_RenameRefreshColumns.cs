using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class RenameRefreshColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RefreshAfterXWeeks",
                table: "user_variation",
                newName: "PadRefreshXWeeks");

            migrationBuilder.RenameColumn(
                name: "DelayRefreshXWeeks",
                table: "user_variation",
                newName: "LagRefreshXWeeks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PadRefreshXWeeks",
                table: "user_variation",
                newName: "RefreshAfterXWeeks");

            migrationBuilder.RenameColumn(
                name: "LagRefreshXWeeks",
                table: "user_variation",
                newName: "DelayRefreshXWeeks");
        }
    }
}
