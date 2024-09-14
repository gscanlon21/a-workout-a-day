using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameCol4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WeightSecondaryMusclesXTimesLess",
                table: "user",
                newName: "WeightSecondaryXTimesLess");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WeightSecondaryXTimesLess",
                table: "user",
                newName: "WeightSecondaryMusclesXTimesLess");
        }
    }
}
