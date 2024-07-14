using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorImageVerbosity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowStaticImages",
                table: "user");

            migrationBuilder.AddColumn<int>(
                name: "ImageType",
                table: "user",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageType",
                table: "user");

            migrationBuilder.AddColumn<bool>(
                name: "ShowStaticImages",
                table: "user",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
