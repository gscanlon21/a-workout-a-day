using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddIdToUserToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_user_token",
                table: "user_token");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "user_token",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_token",
                table: "user_token",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_user_token_UserId_Token",
                table: "user_token",
                columns: new[] { "UserId", "Token" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_user_token",
                table: "user_token");

            migrationBuilder.DropIndex(
                name: "IX_user_token_UserId_Token",
                table: "user_token");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "user_token");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_token",
                table: "user_token",
                columns: new[] { "UserId", "Token" });
        }
    }
}
