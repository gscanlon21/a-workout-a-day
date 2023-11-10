using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class FlattenUserPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_preference");

            migrationBuilder.AddColumn<int>(
                name: "AtLeastXUniqueMusclesPerExercise_Accessory",
                table: "user",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AtLeastXUniqueMusclesPerExercise_Flexibility",
                table: "user",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AtLeastXUniqueMusclesPerExercise_Mobility",
                table: "user",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IgnorePrerequisites",
                table: "user",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "WeightIsolationXTimesMore",
                table: "user",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "WeightSecondaryMusclesXTimesLess",
                table: "user",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AtLeastXUniqueMusclesPerExercise_Accessory",
                table: "user");

            migrationBuilder.DropColumn(
                name: "AtLeastXUniqueMusclesPerExercise_Flexibility",
                table: "user");

            migrationBuilder.DropColumn(
                name: "AtLeastXUniqueMusclesPerExercise_Mobility",
                table: "user");

            migrationBuilder.DropColumn(
                name: "IgnorePrerequisites",
                table: "user");

            migrationBuilder.DropColumn(
                name: "WeightIsolationXTimesMore",
                table: "user");

            migrationBuilder.DropColumn(
                name: "WeightSecondaryMusclesXTimesLess",
                table: "user");

            migrationBuilder.CreateTable(
                name: "user_preference",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    AtLeastXUniqueMusclesPerExercise_Accessory = table.Column<int>(type: "integer", nullable: false),
                    AtLeastXUniqueMusclesPerExercise_Flexibility = table.Column<int>(type: "integer", nullable: false),
                    AtLeastXUniqueMusclesPerExercise_Mobility = table.Column<int>(type: "integer", nullable: false),
                    IgnorePrerequisites = table.Column<bool>(type: "boolean", nullable: false),
                    WeightIsolationXTimesMore = table.Column<double>(type: "double precision", nullable: false),
                    WeightSecondaryMusclesXTimesLess = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_preference", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_user_preference_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Advanced workout settings");
        }
    }
}
