using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorToMinFontSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FontSizeAdjust",
                table: "user");

            migrationBuilder.AddColumn<double>(
                name: "MinFontSize",
                table: "user",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinFontSize",
                table: "user");

            migrationBuilder.AddColumn<int>(
                name: "FontSizeAdjust",
                table: "user",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
