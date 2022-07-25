using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class RemoveEquipmentMappingTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquipmentEquipmentGroup");

            migrationBuilder.DropTable(
                name: "EquipmentGroupVariation");

            migrationBuilder.DropTable(
                name: "EquipmentGroup");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquipmentVariation");

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
                name: "IX_EquipmentEquipmentGroup_EquipmentId",
                table: "EquipmentEquipmentGroup",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentGroupVariation_VariationsId",
                table: "EquipmentGroupVariation",
                column: "VariationsId");
        }
    }
}
