using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class ImprovePrerequisites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Proficiency",
                table: "exercise");

            migrationBuilder.AddColumn<int>(
                name: "Proficiency",
                table: "exercise_prerequisite",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Proficiency",
                table: "exercise_prerequisite");

            migrationBuilder.AddColumn<int>(
                name: "Proficiency",
                table: "exercise",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
