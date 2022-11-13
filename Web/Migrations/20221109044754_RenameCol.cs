using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    public partial class RenameCol : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NewsletterRotation_ExerciseType",
                table: "newsletter",
                newName: "NewsletterRotation_NewsletterType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NewsletterRotation_NewsletterType",
                table: "newsletter",
                newName: "NewsletterRotation_ExerciseType");
        }
    }
}
