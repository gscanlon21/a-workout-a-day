using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class NonNullableNewsletterUserId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_newsletter_user_UserId",
                table: "newsletter");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "newsletter",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_newsletter_user_UserId",
                table: "newsletter",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_newsletter_user_UserId",
                table: "newsletter");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "newsletter",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_newsletter_user_UserId",
                table: "newsletter",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id");
        }
    }
}
