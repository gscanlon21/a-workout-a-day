using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    public partial class RemoveMuscleMovement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MuscleMovement",
                table: "variation");

            migrationBuilder.DropColumn(
                name: "MuscleMovement",
                table: "equipment_group");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MuscleMovement",
                table: "variation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MuscleMovement",
                table: "equipment_group",
                type: "integer",
                nullable: true);
        }
    }
}
