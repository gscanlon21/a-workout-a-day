using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class MoveEquipmentToVariations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Newsletter");

            migrationBuilder.DropColumn(
                name: "Equipment",
                table: "Exercise");

            migrationBuilder.DropColumn(
                name: "MuscleContractions",
                table: "Exercise");

            migrationBuilder.AddColumn<int>(
                name: "Equipment",
                table: "Variation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MuscleContractions",
                table: "Variation",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Equipment",
                table: "Variation");

            migrationBuilder.DropColumn(
                name: "MuscleContractions",
                table: "Variation");

            migrationBuilder.AddColumn<int>(
                name: "Equipment",
                table: "Exercise",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MuscleContractions",
                table: "Exercise",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Newsletter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Equipment = table.Column<int>(type: "integer", nullable: false),
                    ExerciseType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Newsletter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Newsletter_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                },
                comment: "A day's workout routine");

            migrationBuilder.CreateIndex(
                name: "IX_Newsletter_UserId",
                table: "Newsletter",
                column: "UserId");
        }
    }
}
