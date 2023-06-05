using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class BetterNewsletterMuscleVolumes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OffDayStretchingEnabled",
                table: "user");

            migrationBuilder.DropColumn(
                name: "IntensityLevel",
                table: "newsletter");

            migrationBuilder.DropColumn(
                name: "IsNewToFitness",
                table: "newsletter");

            migrationBuilder.AddColumn<bool>(
                name: "OffDayStretching",
                table: "user",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OffDayStretching",
                table: "user");

            migrationBuilder.AddColumn<DateOnly>(
                name: "OffDayStretchingEnabled",
                table: "user",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntensityLevel",
                table: "newsletter",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsNewToFitness",
                table: "newsletter",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
