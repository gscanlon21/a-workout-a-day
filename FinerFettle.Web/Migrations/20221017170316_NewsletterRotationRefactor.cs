using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class NewsletterRotationRefactor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExerciseRotation_id",
                table: "newsletter",
                newName: "NewsletterRotation_Id");

            migrationBuilder.RenameColumn(
                name: "ExerciseRotation_MuscleGroups",
                table: "newsletter",
                newName: "NewsletterRotation_MuscleGroups");

            migrationBuilder.RenameColumn(
                name: "ExerciseRotation_ExerciseType",
                table: "newsletter",
                newName: "NewsletterRotation_NewsletterType");

            migrationBuilder.AddColumn<int>(
                name: "NewsletterRotation_IntensityLevel",
                table: "newsletter",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewsletterRotation_IntensityLevel",
                table: "newsletter");

            migrationBuilder.RenameColumn(
                name: "NewsletterRotation_MuscleGroups",
                table: "newsletter",
                newName: "ExerciseRotation_MuscleGroups");

            migrationBuilder.RenameColumn(
                name: "NewsletterRotation_Id",
                table: "newsletter",
                newName: "ExerciseRotation_id");

            migrationBuilder.RenameColumn(
                name: "NewsletterRotation_NewsletterType",
                table: "newsletter",
                newName: "ExerciseRotation_ExerciseType");
        }
    }
}
