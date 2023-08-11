using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserFootnotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_footnote_user_UserId",
                table: "footnote");

            migrationBuilder.DropPrimaryKey(
                name: "PK_footnote",
                table: "footnote");

            migrationBuilder.RenameTable(
                name: "footnote",
                newName: "user_footnote");

            migrationBuilder.RenameIndex(
                name: "IX_footnote_UserId",
                table: "user_footnote",
                newName: "IX_user_footnote_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_footnote",
                table: "user_footnote",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_footnote_user_UserId",
                table: "user_footnote",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_footnote_user_UserId",
                table: "user_footnote");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_footnote",
                table: "user_footnote");

            migrationBuilder.RenameTable(
                name: "user_footnote",
                newName: "footnote");

            migrationBuilder.RenameIndex(
                name: "IX_user_footnote_UserId",
                table: "footnote",
                newName: "IX_footnote_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_footnote",
                table: "footnote",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_footnote_user_UserId",
                table: "footnote",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id");
        }
    }
}
