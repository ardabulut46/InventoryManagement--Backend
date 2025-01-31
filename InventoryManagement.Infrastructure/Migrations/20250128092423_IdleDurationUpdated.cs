using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    public partial class IdleDurationUpdated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add a temporary column to store ticks as bigint
            migrationBuilder.AddColumn<long>(
                name: "TempIdleDuration",
                table: "Tickets",
                type: "bigint",
                nullable: true);

            // Step 2: Convert existing time data to ticks (seconds * 10^7)
            migrationBuilder.Sql(@"
                UPDATE Tickets 
                SET TempIdleDuration = 
                    CASE 
                        WHEN IdleDuration IS NOT NULL 
                        THEN DATEDIFF_BIG(SECOND, '00:00:00', IdleDuration) * 10000000 
                        ELSE NULL 
                    END
            ");

            // Step 3: Drop the original time column
            migrationBuilder.DropColumn(
                name: "IdleDuration",
                table: "Tickets");

            // Step 4: Rename the temporary column to IdleDuration
            migrationBuilder.RenameColumn(
                name: "TempIdleDuration",
                table: "Tickets",
                newName: "IdleDuration");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add a temporary column to restore time data
            migrationBuilder.AddColumn<TimeSpan>(
                name: "TempIdleDuration",
                table: "Tickets",
                type: "time",
                nullable: true);

            // Step 2: Convert ticks back to time (seconds = ticks / 10^7)
            migrationBuilder.Sql(@"
                UPDATE Tickets 
                SET TempIdleDuration = 
                    CASE 
                        WHEN IdleDuration IS NOT NULL 
                        THEN DATEADD(SECOND, IdleDuration / 10000000, CAST('00:00:00' AS time)) 
                        ELSE NULL 
                    END
            ");

            // Step 3: Drop the bigint column
            migrationBuilder.DropColumn(
                name: "IdleDuration",
                table: "Tickets");

            // Step 4: Rename the temporary column to IdleDuration
            migrationBuilder.RenameColumn(
                name: "TempIdleDuration",
                table: "Tickets",
                newName: "IdleDuration");
        }
    }
}