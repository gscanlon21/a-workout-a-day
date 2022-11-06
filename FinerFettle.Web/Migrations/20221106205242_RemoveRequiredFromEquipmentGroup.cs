using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class RemoveRequiredFromEquipmentGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Required",
                table: "equipment_group");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Required",
                table: "equipment_group",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
