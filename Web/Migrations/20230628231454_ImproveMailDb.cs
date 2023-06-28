using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class ImproveMailDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sent",
                table: "user_newsletter");

            migrationBuilder.RenameColumn(
                name: "Error",
                table: "user_newsletter",
                newName: "LastError");

            migrationBuilder.AddColumn<int>(
                name: "EmailStatus",
                table: "user_newsletter",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SendAfter",
                table: "user_newsletter",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "SendAttempts",
                table: "user_newsletter",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailStatus",
                table: "user_newsletter");

            migrationBuilder.DropColumn(
                name: "SendAfter",
                table: "user_newsletter");

            migrationBuilder.DropColumn(
                name: "SendAttempts",
                table: "user_newsletter");

            migrationBuilder.RenameColumn(
                name: "LastError",
                table: "user_newsletter",
                newName: "Error");

            migrationBuilder.AddColumn<bool>(
                name: "Sent",
                table: "user_newsletter",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
