using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxSecsToProficiency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncludeAdjunct",
                table: "user");

            migrationBuilder.RenameColumn(
                name: "Proficiency_Secs",
                table: "intensity",
                newName: "Proficiency_MinSecs");

            migrationBuilder.AlterColumn<int>(
                name: "Proficiency_Sets",
                table: "intensity",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "Proficiency_MaxSecs",
                table: "intensity",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Proficiency_MaxSecs",
                table: "intensity");

            migrationBuilder.RenameColumn(
                name: "Proficiency_MinSecs",
                table: "intensity",
                newName: "Proficiency_Secs");

            migrationBuilder.AddColumn<bool>(
                name: "IncludeAdjunct",
                table: "user",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "Proficiency_Sets",
                table: "intensity",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
