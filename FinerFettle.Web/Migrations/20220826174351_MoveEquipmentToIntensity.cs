using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class MoveEquipmentToIntensity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentGroup_Variation_VariationsId",
                table: "EquipmentGroup");

            migrationBuilder.RenameColumn(
                name: "VariationsId",
                table: "EquipmentGroup",
                newName: "IntensityId");

            migrationBuilder.RenameIndex(
                name: "IX_EquipmentGroup_VariationsId",
                table: "EquipmentGroup",
                newName: "IX_EquipmentGroup_IntensityId");

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentGroup_Intensity_IntensityId",
                table: "EquipmentGroup",
                column: "IntensityId",
                principalTable: "Intensity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentGroup_Intensity_IntensityId",
                table: "EquipmentGroup");

            migrationBuilder.RenameColumn(
                name: "IntensityId",
                table: "EquipmentGroup",
                newName: "VariationsId");

            migrationBuilder.RenameIndex(
                name: "IX_EquipmentGroup_IntensityId",
                table: "EquipmentGroup",
                newName: "IX_EquipmentGroup_VariationsId");

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentGroup_Variation_VariationsId",
                table: "EquipmentGroup",
                column: "VariationsId",
                principalTable: "Variation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
