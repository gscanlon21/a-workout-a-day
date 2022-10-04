using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class RecoveryMuscle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecoveryMuscle",
                table: "User",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecoveryMuscle",
                table: "User");
        }
    }
}
