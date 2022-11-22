using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class ReworkMuscles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SecondaryMuscles",
                table: "variation",
                newName: "StretchMuscles");

            migrationBuilder.RenameColumn(
                name: "PrimaryMuscles",
                table: "variation",
                newName: "StrengthMuscles");

            migrationBuilder.AddColumn<int>(
                name: "StabilityMuscles",
                table: "variation",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StabilityMuscles",
                table: "variation");

            migrationBuilder.RenameColumn(
                name: "StretchMuscles",
                table: "variation",
                newName: "SecondaryMuscles");

            migrationBuilder.RenameColumn(
                name: "StrengthMuscles",
                table: "variation",
                newName: "PrimaryMuscles");
        }
    }
}
