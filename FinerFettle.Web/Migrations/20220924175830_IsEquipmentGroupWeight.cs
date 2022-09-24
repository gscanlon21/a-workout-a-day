using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class IsEquipmentGroupWeight : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsWeight",
                table: "Equipment");

            migrationBuilder.AddColumn<bool>(
                name: "IsWeight",
                table: "EquipmentGroup",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsWeight",
                table: "EquipmentGroup");

            migrationBuilder.AddColumn<bool>(
                name: "IsWeight",
                table: "Equipment",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
