using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class MoveStretchIntoIntensityTable2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Variation_Intensity_IntensityId",
                table: "Variation");

            migrationBuilder.DropIndex(
                name: "IX_Variation_IntensityId",
                table: "Variation");

            migrationBuilder.DropColumn(
                name: "IntensityId",
                table: "Variation");

            migrationBuilder.AddColumn<int>(
                name: "VariationId",
                table: "Intensity",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Intensity_VariationId",
                table: "Intensity",
                column: "VariationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Intensity_Variation_VariationId",
                table: "Intensity",
                column: "VariationId",
                principalTable: "Variation",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Intensity_Variation_VariationId",
                table: "Intensity");

            migrationBuilder.DropIndex(
                name: "IX_Intensity_VariationId",
                table: "Intensity");

            migrationBuilder.DropColumn(
                name: "VariationId",
                table: "Intensity");

            migrationBuilder.AddColumn<int>(
                name: "IntensityId",
                table: "Variation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Variation_IntensityId",
                table: "Variation",
                column: "IntensityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Variation_Intensity_IntensityId",
                table: "Variation",
                column: "IntensityId",
                principalTable: "Intensity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
