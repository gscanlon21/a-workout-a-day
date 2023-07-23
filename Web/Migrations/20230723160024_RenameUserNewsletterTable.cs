using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserNewsletterTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_newsletter_user_UserId",
                table: "user_newsletter");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_newsletter",
                table: "user_newsletter");

            migrationBuilder.RenameTable(
                name: "user_newsletter",
                newName: "user_email");

            migrationBuilder.RenameIndex(
                name: "IX_user_newsletter_UserId",
                table: "user_email",
                newName: "IX_user_email_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_email",
                table: "user_email",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_email_user_UserId",
                table: "user_email",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_email_user_UserId",
                table: "user_email");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_email",
                table: "user_email");

            migrationBuilder.RenameTable(
                name: "user_email",
                newName: "user_newsletter");

            migrationBuilder.RenameIndex(
                name: "IX_user_email_UserId",
                table: "user_newsletter",
                newName: "IX_user_newsletter_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_newsletter",
                table: "user_newsletter",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_newsletter_user_UserId",
                table: "user_newsletter",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
