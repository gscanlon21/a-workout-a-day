using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class ChildEquipmentGroups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "equipment_group",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_equipment_group_ParentId",
                table: "equipment_group",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_equipment_group_equipment_group_ParentId",
                table: "equipment_group",
                column: "ParentId",
                principalTable: "equipment_group",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_equipment_group_equipment_group_ParentId",
                table: "equipment_group");

            migrationBuilder.DropIndex(
                name: "IX_equipment_group_ParentId",
                table: "equipment_group");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "equipment_group");
        }
    }
}
