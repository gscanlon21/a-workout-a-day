using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class MoveEquipmentToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Equipment",
                table: "Variation");

            migrationBuilder.DropColumn(
                name: "Equipment",
                table: "User");

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
                name: "Equipment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    EquipmentGroupId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equipment_EquipmentGroup_EquipmentGroupId",
                        column: x => x.EquipmentGroupId,
                        principalTable: "EquipmentGroup",
                        principalColumn: "Id");
                },
                comment: "Equipment used in an exercise");

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

            migrationBuilder.CreateTable(
                name: "EquipmentUser",
                columns: table => new
                {
                    EquipmentId = table.Column<int>(type: "integer", nullable: false),
                    UsersId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentUser", x => new { x.EquipmentId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_EquipmentUser_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentUser_User_UsersId",
                        column: x => x.UsersId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_EquipmentGroupId",
                table: "Equipment",
                column: "EquipmentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentGroupVariation_VariationsId",
                table: "EquipmentGroupVariation",
                column: "VariationsId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentUser_UsersId",
                table: "EquipmentUser",
                column: "UsersId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquipmentGroupVariation");

            migrationBuilder.DropTable(
                name: "EquipmentUser");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropTable(
                name: "EquipmentGroup");

            migrationBuilder.AddColumn<int>(
                name: "Equipment",
                table: "Variation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Equipment",
                table: "User",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
