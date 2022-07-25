using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    public partial class AddEquipmentUserMapping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentUser_User_UsersId",
                table: "EquipmentUser");

            migrationBuilder.RenameColumn(
                name: "UsersId",
                table: "EquipmentUser",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_EquipmentUser_UsersId",
                table: "EquipmentUser",
                newName: "IX_EquipmentUser_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentUser_User_UserId",
                table: "EquipmentUser",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentUser_User_UserId",
                table: "EquipmentUser");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "EquipmentUser",
                newName: "UsersId");

            migrationBuilder.RenameIndex(
                name: "IX_EquipmentUser_UserId",
                table: "EquipmentUser",
                newName: "IX_EquipmentUser_UsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentUser_User_UsersId",
                table: "EquipmentUser",
                column: "UsersId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
