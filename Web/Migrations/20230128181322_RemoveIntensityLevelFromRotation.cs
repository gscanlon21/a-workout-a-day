using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIntensityLevelFromRotation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewsletterRotation_IntensityLevel",
                table: "newsletter");

            migrationBuilder.DropColumn(
                name: "NewsletterRotation_NewsletterType",
                table: "newsletter");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NewsletterRotation_IntensityLevel",
                table: "newsletter",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NewsletterRotation_NewsletterType",
                table: "newsletter",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
