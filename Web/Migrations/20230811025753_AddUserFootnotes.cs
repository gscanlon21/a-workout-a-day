using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFootnotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "footnote",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_footnote_UserId",
                table: "footnote",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_footnote_user_UserId",
                table: "footnote",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_footnote_user_UserId",
                table: "footnote");

            migrationBuilder.DropIndex(
                name: "IX_footnote_UserId",
                table: "footnote");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "footnote");
        }
    }
}
