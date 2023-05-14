using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class RefactorRehab : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrehabFocus",
                table: "exercise_variation");

            migrationBuilder.DropColumn(
                name: "RehabFocus",
                table: "exercise_variation");

            migrationBuilder.AddColumn<int>(
                name: "MobilityJoints",
                table: "variation",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MobilityJoints",
                table: "variation");

            migrationBuilder.AddColumn<int>(
                name: "PrehabFocus",
                table: "exercise_variation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RehabFocus",
                table: "exercise_variation",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
