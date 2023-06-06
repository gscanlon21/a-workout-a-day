using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class ClarifyUserPropNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StretchingMuscles",
                table: "user",
                newName: "MobilityMuscles");

            migrationBuilder.RenameColumn(
                name: "OffDayStretching",
                table: "user",
                newName: "SendMobilityWorkouts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SendMobilityWorkouts",
                table: "user",
                newName: "OffDayStretching");

            migrationBuilder.RenameColumn(
                name: "MobilityMuscles",
                table: "user",
                newName: "StretchingMuscles");
        }
    }
}
