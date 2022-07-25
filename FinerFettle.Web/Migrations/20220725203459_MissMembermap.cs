using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class MissMembermap : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipment_EquipmentGroup_EquipmentGroupId",
                table: "Equipment");

            migrationBuilder.DropForeignKey(
                name: "FK_Variation_Equipment_EquipmentId",
                table: "Variation");

            migrationBuilder.DropIndex(
                name: "IX_Variation_EquipmentId",
                table: "Variation");

            migrationBuilder.DropIndex(
                name: "IX_Equipment_EquipmentGroupId",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "EquipmentId",
                table: "Variation");

            migrationBuilder.DropColumn(
                name: "EquipmentGroupId",
                table: "Equipment");

            migrationBuilder.CreateTable(
                name: "EquipmentEquipmentGroup",
                columns: table => new
                {
                    EquipmentGroupsId = table.Column<int>(type: "integer", nullable: false),
                    EquipmentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentEquipmentGroup", x => new { x.EquipmentGroupsId, x.EquipmentId });
                    table.ForeignKey(
                        name: "FK_EquipmentEquipmentGroup_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentEquipmentGroup_EquipmentGroup_EquipmentGroupsId",
                        column: x => x.EquipmentGroupsId,
                        principalTable: "EquipmentGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentEquipmentGroup_EquipmentId",
                table: "EquipmentEquipmentGroup",
                column: "EquipmentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquipmentEquipmentGroup");

            migrationBuilder.AddColumn<int>(
                name: "EquipmentId",
                table: "Variation",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EquipmentGroupId",
                table: "Equipment",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Variation_EquipmentId",
                table: "Variation",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_EquipmentGroupId",
                table: "Equipment",
                column: "EquipmentGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipment_EquipmentGroup_EquipmentGroupId",
                table: "Equipment",
                column: "EquipmentGroupId",
                principalTable: "EquipmentGroup",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Variation_Equipment_EquipmentId",
                table: "Variation",
                column: "EquipmentId",
                principalTable: "Equipment",
                principalColumn: "Id");
        }
    }
}
