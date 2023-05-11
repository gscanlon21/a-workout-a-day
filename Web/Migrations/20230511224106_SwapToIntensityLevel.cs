using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SwapToIntensityLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StrengtheningPreference",
                table: "user",
                newName: "IntensityLevel");

            migrationBuilder.RenameColumn(
                name: "StrengtheningPreference",
                table: "newsletter",
                newName: "IntensityLevel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IntensityLevel",
                table: "user",
                newName: "StrengtheningPreference");

            migrationBuilder.RenameColumn(
                name: "IntensityLevel",
                table: "newsletter",
                newName: "StrengtheningPreference");
        }
    }
}
