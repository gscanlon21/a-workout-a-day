using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class RemoveForm2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Form_Variation_VariationId",
                table: "Form");

            migrationBuilder.DropTable(
                name: "EquipmentGroupForm");

            migrationBuilder.DropIndex(
                name: "IX_Form_VariationId",
                table: "Form");

            migrationBuilder.DropColumn(
                name: "VariationId",
                table: "Form");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Exercise");

            migrationBuilder.AlterTable(
                name: "Form",
                comment: "Progressions of an exercise",
                oldComment: "Instructions of various ways to do a variation");

            migrationBuilder.AddColumn<string>(
                name: "Instruction",
                table: "Variation",
                type: "text",
                nullable: false,
                defaultValue: "");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquipmentGroupVariation");

            migrationBuilder.DropColumn(
                name: "Instruction",
                table: "Variation");

            migrationBuilder.AlterTable(
                name: "Form",
                comment: "Instructions of various ways to do a variation",
                oldComment: "Progressions of an exercise");

            migrationBuilder.AddColumn<int>(
                name: "VariationId",
                table: "Form",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Exercise",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "EquipmentGroupForm",
                columns: table => new
                {
                    EquipmentGroupsId = table.Column<int>(type: "integer", nullable: false),
                    FormsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentGroupForm", x => new { x.EquipmentGroupsId, x.FormsId });
                    table.ForeignKey(
                        name: "FK_EquipmentGroupForm_EquipmentGroup_EquipmentGroupsId",
                        column: x => x.EquipmentGroupsId,
                        principalTable: "EquipmentGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentGroupForm_Form_FormsId",
                        column: x => x.FormsId,
                        principalTable: "Form",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Form_VariationId",
                table: "Form",
                column: "VariationId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentGroupForm_FormsId",
                table: "EquipmentGroupForm",
                column: "FormsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Form_Variation_VariationId",
                table: "Form",
                column: "VariationId",
                principalTable: "Variation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
