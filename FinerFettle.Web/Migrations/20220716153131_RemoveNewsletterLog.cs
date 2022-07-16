using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class RemoveNewsletterLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercise_Newsletter_NewsletterId",
                table: "Exercise");

            migrationBuilder.DropIndex(
                name: "IX_Exercise_NewsletterId",
                table: "Exercise");

            migrationBuilder.DropColumn(
                name: "NewsletterId",
                table: "Exercise");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NewsletterId",
                table: "Exercise",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_NewsletterId",
                table: "Exercise",
                column: "NewsletterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercise_Newsletter_NewsletterId",
                table: "Exercise",
                column: "NewsletterId",
                principalTable: "Newsletter",
                principalColumn: "Id");
        }
    }
}
