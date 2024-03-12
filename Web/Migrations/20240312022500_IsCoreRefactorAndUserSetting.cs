using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class IsCoreRefactorAndUserSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshAfter",
                table: "user_variation");

            migrationBuilder.RenameColumn(
                name: "WeightCoreExercisesXTimesMore",
                table: "user",
                newName: "WeightPrimaryExercisesXTimesMore");

            migrationBuilder.RenameColumn(
                name: "IsCore",
                table: "exercise",
                newName: "IsPrimary");

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "user_exercise",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "user_exercise");

            migrationBuilder.RenameColumn(
                name: "WeightPrimaryExercisesXTimesMore",
                table: "user",
                newName: "WeightCoreExercisesXTimesMore");

            migrationBuilder.RenameColumn(
                name: "IsPrimary",
                table: "exercise",
                newName: "IsCore");

            migrationBuilder.AddColumn<DateOnly>(
                name: "RefreshAfter",
                table: "user_variation",
                type: "date",
                nullable: true);
        }
    }
}
