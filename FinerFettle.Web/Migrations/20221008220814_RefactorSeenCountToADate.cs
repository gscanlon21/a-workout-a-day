using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class RefactorSeenCountToADate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SeenCount",
                table: "user_variation");

            migrationBuilder.DropColumn(
                name: "SeenCount",
                table: "user_exercise");

            migrationBuilder.AddColumn<DateOnly>(
                name: "LastSeen",
                table: "user_variation",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "LastSeen",
                table: "user_exercise",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSeen",
                table: "user_variation");

            migrationBuilder.DropColumn(
                name: "LastSeen",
                table: "user_exercise");

            migrationBuilder.AddColumn<int>(
                name: "SeenCount",
                table: "user_variation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SeenCount",
                table: "user_exercise",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
