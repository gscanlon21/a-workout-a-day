using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_preference",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    IgnorePrerequisites = table.Column<bool>(type: "boolean", nullable: false),
                    AtLeastXUniqueMusclesPerExercise_Mobility = table.Column<int>(type: "integer", nullable: false),
                    AtLeastXUniqueMusclesPerExercise_Flexibility = table.Column<int>(type: "integer", nullable: false),
                    AtLeastXUniqueMusclesPerExercise_Accessory = table.Column<int>(type: "integer", nullable: false),
                    WeightSecondaryMusclesXTimesLess = table.Column<double>(type: "double precision", nullable: false),
                    WeightIsolationXTimesMore = table.Column<double>(type: "double precision", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_preference");
        }
    }
}
