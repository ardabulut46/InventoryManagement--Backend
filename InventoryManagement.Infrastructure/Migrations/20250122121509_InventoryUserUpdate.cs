using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InventoryUserUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastUserId",
                table: "Inventories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_LastUserId",
                table: "Inventories",
                column: "LastUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_AspNetUsers_LastUserId",
                table: "Inventories",
                column: "LastUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_AspNetUsers_LastUserId",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_LastUserId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "LastUserId",
                table: "Inventories");
        }
    }
}
