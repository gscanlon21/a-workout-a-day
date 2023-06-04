using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SwitchOffDayStretchingToDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OffDayStretching",
                table: "user");

            migrationBuilder.AddColumn<DateOnly>(
                name: "OffDayStretchingEnabled",
                table: "user",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OffDayStretchingEnabled",
                table: "user");

            migrationBuilder.AddColumn<bool>(
                name: "OffDayStretching",
                table: "user",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
