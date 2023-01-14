using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class EquipmentInstructionLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "equipment_group_instruction",
                columns: table => new
                {
                    Location = table.Column<int>(type: "integer", nullable: false),
                    EquipmentGroupId = table.Column<int>(type: "integer", nullable: false),
                    Instruction = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipment_group_instruction", x => new { x.Location, x.EquipmentGroupId });
                    table.ForeignKey(
                        name: "FK_equipment_group_instruction_equipment_group_EquipmentGroupId",
                        column: x => x.EquipmentGroupId,
                        principalTable: "equipment_group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Instructions that can be switched out for one another");

            migrationBuilder.CreateIndex(
                name: "IX_equipment_group_instruction_EquipmentGroupId",
                table: "equipment_group_instruction",
                column: "EquipmentGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "equipment_group_instruction");
        }
    }
}
