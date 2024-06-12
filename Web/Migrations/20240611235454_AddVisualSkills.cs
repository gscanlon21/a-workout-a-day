using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddVisualSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Groups",
                table: "exercise",
                newName: "Skills");

            migrationBuilder.AddColumn<int>(
                name: "SkillType",
                table: "exercise",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SkillType",
                table: "exercise");

            migrationBuilder.RenameColumn(
                name: "Skills",
                table: "exercise",
                newName: "Groups");
        }
    }
}
