using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPrefToDisableNewsletter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SendMobilityWorkouts",
                table: "user",
                newName: "IncludeMobilityWorkouts");

            migrationBuilder.AddColumn<bool>(
                name: "SendEmailWorkouts",
                table: "user",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SendEmailWorkouts",
                table: "user");

            migrationBuilder.RenameColumn(
                name: "IncludeMobilityWorkouts",
                table: "user",
                newName: "SendMobilityWorkouts");
        }
    }
}
