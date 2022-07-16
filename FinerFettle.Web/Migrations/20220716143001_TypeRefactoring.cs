using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class TypeRefactoring : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Exercise");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Exercise",
                newName: "MuscleContractions");

            migrationBuilder.RenameColumn(
                name: "Form",
                table: "Exercise",
                newName: "ExerciseType");

            migrationBuilder.AddColumn<int>(
                name: "VariationType",
                table: "Variation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Muscles",
                table: "Exercise",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "NewsletterId",
                table: "Exercise",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Newsletter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Newsletter", x => x.Id);
                },
                comment: "A day's workout routine");

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_NewsletterId",
                table: "Exercise",
                column: "NewsletterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercise_Newsletter_NewsletterId",
                table: "Exercise",
                column: "NewsletterId",
                principalTable: "Newsletter",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercise_Newsletter_NewsletterId",
                table: "Exercise");

            migrationBuilder.DropTable(
                name: "Newsletter");

            migrationBuilder.DropIndex(
                name: "IX_Exercise_NewsletterId",
                table: "Exercise");

            migrationBuilder.DropColumn(
                name: "VariationType",
                table: "Variation");

            migrationBuilder.DropColumn(
                name: "Muscles",
                table: "Exercise");

            migrationBuilder.DropColumn(
                name: "NewsletterId",
                table: "Exercise");

            migrationBuilder.RenameColumn(
                name: "MuscleContractions",
                table: "Exercise",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "ExerciseType",
                table: "Exercise",
                newName: "Form");

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Exercise",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
