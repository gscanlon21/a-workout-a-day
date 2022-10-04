using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class RefactorRoutine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ExerciseUserProgression",
                table: "ExerciseUserProgression");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseUserProgression_UserId",
                table: "ExerciseUserProgression");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EquipmentUser",
                table: "EquipmentUser");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentUser_UserId",
                table: "EquipmentUser");

            migrationBuilder.AddColumn<string>(
                name: "DisabledReason",
                table: "Intensity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Ignore",
                table: "ExerciseUserProgression",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExerciseUserProgression",
                table: "ExerciseUserProgression",
                columns: new[] { "UserId", "ExerciseId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_EquipmentUser",
                table: "EquipmentUser",
                columns: new[] { "UserId", "EquipmentId" });

            migrationBuilder.CreateTable(
                name: "UserIntensity",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    IntensityId = table.Column<int>(type: "integer", nullable: false),
                    SeenCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserIntensity", x => new { x.UserId, x.IntensityId });
                    table.ForeignKey(
                        name: "FK_UserIntensity_Intensity_IntensityId",
                        column: x => x.IntensityId,
                        principalTable: "Intensity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserIntensity_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "User's intensity stats");

            migrationBuilder.CreateTable(
                name: "UserVariation",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    VariationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVariation", x => new { x.UserId, x.VariationId });
                    table.ForeignKey(
                        name: "FK_UserVariation_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserVariation_Variation_VariationId",
                        column: x => x.VariationId,
                        principalTable: "Variation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "User's variation excluding");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseUserProgression_ExerciseId",
                table: "ExerciseUserProgression",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentUser_EquipmentId",
                table: "EquipmentUser",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserIntensity_IntensityId",
                table: "UserIntensity",
                column: "IntensityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVariation_VariationId",
                table: "UserVariation",
                column: "VariationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserIntensity");

            migrationBuilder.DropTable(
                name: "UserVariation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExerciseUserProgression",
                table: "ExerciseUserProgression");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseUserProgression_ExerciseId",
                table: "ExerciseUserProgression");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EquipmentUser",
                table: "EquipmentUser");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentUser_EquipmentId",
                table: "EquipmentUser");

            migrationBuilder.DropColumn(
                name: "DisabledReason",
                table: "Intensity");

            migrationBuilder.DropColumn(
                name: "Ignore",
                table: "ExerciseUserProgression");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExerciseUserProgression",
                table: "ExerciseUserProgression",
                columns: new[] { "ExerciseId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_EquipmentUser",
                table: "EquipmentUser",
                columns: new[] { "EquipmentId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseUserProgression_UserId",
                table: "ExerciseUserProgression",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentUser_UserId",
                table: "EquipmentUser",
                column: "UserId");
        }
    }
}
