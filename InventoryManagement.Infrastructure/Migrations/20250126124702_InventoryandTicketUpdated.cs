using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InventoryandTicketUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "IdleDuration",
                table: "Tickets",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedUserId",
                table: "Inventories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_CreatedUserId",
                table: "Inventories",
                column: "CreatedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_AspNetUsers_CreatedUserId",
                table: "Inventories",
                column: "CreatedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_AspNetUsers_CreatedUserId",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_CreatedUserId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "IdleDuration",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CreatedUserId",
                table: "Inventories");
        }
    }
}
