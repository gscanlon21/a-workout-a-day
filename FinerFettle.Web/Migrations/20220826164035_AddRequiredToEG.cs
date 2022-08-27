using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class AddRequiredToEG : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquipmentGroupVariation");

            migrationBuilder.DropColumn(
                name: "Progression",
                table: "User");

            migrationBuilder.AddColumn<string>(
                name: "Instruction",
                table: "EquipmentGroup",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Required",
                table: "EquipmentGroup",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "VariationsId",
                table: "EquipmentGroup",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentGroup_VariationsId",
                table: "EquipmentGroup",
                column: "VariationsId");

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentGroup_Variation_VariationsId",
                table: "EquipmentGroup",
                column: "VariationsId",
                principalTable: "Variation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentGroup_Variation_VariationsId",
                table: "EquipmentGroup");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentGroup_VariationsId",
                table: "EquipmentGroup");

            migrationBuilder.DropColumn(
                name: "Instruction",
                table: "EquipmentGroup");

            migrationBuilder.DropColumn(
                name: "Required",
                table: "EquipmentGroup");

            migrationBuilder.DropColumn(
                name: "VariationsId",
                table: "EquipmentGroup");

            migrationBuilder.AddColumn<int>(
                name: "Progression",
                table: "User",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EquipmentGroupVariation",
                columns: table => new
                {
                    EquipmentGroupsId = table.Column<int>(type: "integer", nullable: false),
                    VariationsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentGroupVariation", x => new { x.EquipmentGroupsId, x.VariationsId });
                    table.ForeignKey(
                        name: "FK_EquipmentGroupVariation_EquipmentGroup_EquipmentGroupsId",
                        column: x => x.EquipmentGroupsId,
                        principalTable: "EquipmentGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentGroupVariation_Variation_VariationsId",
                        column: x => x.VariationsId,
                        principalTable: "Variation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentGroupVariation_VariationsId",
                table: "EquipmentGroupVariation",
                column: "VariationsId");
        }
    }
}
