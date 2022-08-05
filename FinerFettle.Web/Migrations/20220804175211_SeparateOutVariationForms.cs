using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class SeparateOutVariationForms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Intensity_Variation_VariationId",
                table: "Intensity");

            migrationBuilder.DropForeignKey(
                name: "FK_Variation_Exercise_ExerciseId",
                table: "Variation");

            migrationBuilder.DropTable(
                name: "EquipmentGroupVariation");

            migrationBuilder.AlterColumn<int>(
                name: "ExerciseId",
                table: "Variation",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MobilityMuscles",
                table: "User",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "PrefersEccentricExercises",
                table: "User",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PrefersWeightedExercises",
                table: "User",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RecoveryMuscles",
                table: "User",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StrengthMuscles",
                table: "User",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "ExerciseRotation_MuscleGroups",
                table: "Newsletter",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "VariationId",
                table: "Intensity",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Form",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Instruction = table.Column<string>(type: "text", nullable: false),
                    VariationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Form", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Form_Variation_VariationId",
                        column: x => x.VariationId,
                        principalTable: "Variation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Instructions of various ways to do a variation");

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
                name: "IX_EquipmentGroupForm_FormsId",
                table: "EquipmentGroupForm",
                column: "FormsId");

            migrationBuilder.CreateIndex(
                name: "IX_Form_VariationId",
                table: "Form",
                column: "VariationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Intensity_Variation_VariationId",
                table: "Intensity",
                column: "VariationId",
                principalTable: "Variation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Variation_Exercise_ExerciseId",
                table: "Variation",
                column: "ExerciseId",
                principalTable: "Exercise",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Intensity_Variation_VariationId",
                table: "Intensity");

            migrationBuilder.DropForeignKey(
                name: "FK_Variation_Exercise_ExerciseId",
                table: "Variation");

            migrationBuilder.DropTable(
                name: "EquipmentGroupForm");

            migrationBuilder.DropTable(
                name: "Form");

            migrationBuilder.DropColumn(
                name: "MobilityMuscles",
                table: "User");

            migrationBuilder.DropColumn(
                name: "PrefersEccentricExercises",
                table: "User");

            migrationBuilder.DropColumn(
                name: "PrefersWeightedExercises",
                table: "User");

            migrationBuilder.DropColumn(
                name: "RecoveryMuscles",
                table: "User");

            migrationBuilder.DropColumn(
                name: "StrengthMuscles",
                table: "User");

            migrationBuilder.AlterColumn<int>(
                name: "ExerciseId",
                table: "Variation",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "ExerciseRotation_MuscleGroups",
                table: "Newsletter",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "VariationId",
                table: "Intensity",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Intensity_Variation_VariationId",
                table: "Intensity",
                column: "VariationId",
                principalTable: "Variation",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Variation_Exercise_ExerciseId",
                table: "Variation",
                column: "ExerciseId",
                principalTable: "Exercise",
                principalColumn: "Id");
        }
    }
}
