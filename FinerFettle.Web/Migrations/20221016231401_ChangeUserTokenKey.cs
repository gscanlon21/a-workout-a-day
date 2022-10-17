using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class ChangeUserTokenKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_user_token",
                table: "user_token");

            migrationBuilder.DropIndex(
                name: "IX_user_token_UserId",
                table: "user_token");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "user_token");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "user");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_token",
                table: "user_token",
                columns: new[] { "UserId", "Token" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "user",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_token",
                table: "user_token",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_user_token_UserId",
                table: "user_token",
                column: "UserId");
        }
    }
}
