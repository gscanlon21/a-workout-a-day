using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameWeightsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_variation_weight_user_variation_UserVariationId",
                table: "user_variation_weight");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_variation_weight",
                table: "user_variation_weight");

            migrationBuilder.RenameTable(
                name: "user_variation_weight",
                newName: "user_variation_log");

            migrationBuilder.RenameIndex(
                name: "IX_user_variation_weight_UserVariationId",
                table: "user_variation_log",
                newName: "IX_user_variation_log_UserVariationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_variation_log",
                table: "user_variation_log",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_variation_log_user_variation_UserVariationId",
                table: "user_variation_log",
                column: "UserVariationId",
                principalTable: "user_variation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_variation_log_user_variation_UserVariationId",
                table: "user_variation_log");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_variation_log",
                table: "user_variation_log");

            migrationBuilder.RenameTable(
                name: "user_variation_log",
                newName: "user_variation_weight");

            migrationBuilder.RenameIndex(
                name: "IX_user_variation_log_UserVariationId",
                table: "user_variation_weight",
                newName: "IX_user_variation_weight_UserVariationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_variation_weight",
                table: "user_variation_weight",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_variation_weight_user_variation_UserVariationId",
                table: "user_variation_weight",
                column: "UserVariationId",
                principalTable: "user_variation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
