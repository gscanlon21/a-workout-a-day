using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class UserImageTypePref : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageCode",
                table: "variation",
                newName: "StaticImage");

            migrationBuilder.AddColumn<string>(
                name: "AnimatedImage",
                table: "variation",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PreferStaticImages",
                table: "user",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnimatedImage",
                table: "variation");

            migrationBuilder.DropColumn(
                name: "PreferStaticImages",
                table: "user");

            migrationBuilder.RenameColumn(
                name: "StaticImage",
                table: "variation",
                newName: "ImageCode");
        }
    }
}
