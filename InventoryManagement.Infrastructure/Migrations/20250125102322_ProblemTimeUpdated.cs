using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProblemTimeUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SolutionTimes_SolutionTypes_SolutionTypeId",
                table: "SolutionTimes");

            migrationBuilder.RenameColumn(
                name: "SolutionTypeId",
                table: "SolutionTimes",
                newName: "ProblemTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_SolutionTimes_SolutionTypeId",
                table: "SolutionTimes",
                newName: "IX_SolutionTimes_ProblemTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_SolutionTimes_ProblemTypes_ProblemTypeId",
                table: "SolutionTimes",
                column: "ProblemTypeId",
                principalTable: "ProblemTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SolutionTimes_ProblemTypes_ProblemTypeId",
                table: "SolutionTimes");

            migrationBuilder.RenameColumn(
                name: "ProblemTypeId",
                table: "SolutionTimes",
                newName: "SolutionTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_SolutionTimes_ProblemTypeId",
                table: "SolutionTimes",
                newName: "IX_SolutionTimes_SolutionTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_SolutionTimes_SolutionTypes_SolutionTypeId",
                table: "SolutionTimes",
                column: "SolutionTypeId",
                principalTable: "SolutionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
