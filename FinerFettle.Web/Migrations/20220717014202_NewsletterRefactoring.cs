using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class NewsletterRefactoring : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Equipment",
                table: "Newsletter",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ExerciseType",
                table: "Newsletter",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Newsletter",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Newsletter_UserId",
                table: "Newsletter",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Newsletter_User_UserId",
                table: "Newsletter",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Newsletter_User_UserId",
                table: "Newsletter");

            migrationBuilder.DropIndex(
                name: "IX_Newsletter_UserId",
                table: "Newsletter");

            migrationBuilder.DropColumn(
                name: "Equipment",
                table: "Newsletter");

            migrationBuilder.DropColumn(
                name: "ExerciseType",
                table: "Newsletter");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Newsletter");
        }
    }
}
