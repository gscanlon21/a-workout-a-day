using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class BringBackEquipmentGroups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquipmentVariation");

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

            migrationBuilder.CreateTable(
                name: "EquipmentGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentGroup", x => x.Id);
                },
                comment: "Equipment that can be switched out for one another");

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
                name: "IX_Variation_EquipmentId",
                table: "Variation",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_EquipmentGroupId",
                table: "Equipment",
                column: "EquipmentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentGroupVariation_VariationsId",
                table: "EquipmentGroupVariation",
                column: "VariationsId");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipment_EquipmentGroup_EquipmentGroupId",
                table: "Equipment");

            migrationBuilder.DropForeignKey(
                name: "FK_Variation_Equipment_EquipmentId",
                table: "Variation");

            migrationBuilder.DropTable(
                name: "EquipmentGroupVariation");

            migrationBuilder.DropTable(
                name: "EquipmentGroup");

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
                name: "EquipmentVariation",
                columns: table => new
                {
                    EquipmentId = table.Column<int>(type: "integer", nullable: false),
                    VariationsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentVariation", x => new { x.EquipmentId, x.VariationsId });
                    table.ForeignKey(
                        name: "FK_EquipmentVariation_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentVariation_Variation_VariationsId",
                        column: x => x.VariationsId,
                        principalTable: "Variation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentVariation_VariationsId",
                table: "EquipmentVariation",
                column: "VariationsId");
        }
    }
}
