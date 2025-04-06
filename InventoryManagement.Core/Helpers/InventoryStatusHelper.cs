using InventoryManagement.Core.Enums;

namespace InventoryManagement.Core.Helpers;

public static class InventoryStatusHelper
{
    public static InventoryStatus MapStringToInventoryStatus(string status)
    {
        if (string.IsNullOrEmpty(status))
            return InventoryStatus.Available;

        // Try direct parsing first
        if (Enum.TryParse<InventoryStatus>(status, true, out var result))
            return result;

        // Manual mapping for different text values
        return status.ToLower() switch
        {
            "aktif" => InventoryStatus.Available,
            "müsait" => InventoryStatus.Available,
            "kullanımda" => InventoryStatus.InUse,
            "bakımda" => InventoryStatus.UnderMaintenance,
            "emekli" => InventoryStatus.Retired,
            "kayıp" => InventoryStatus.Lost,
            _ => InventoryStatus.Available // Default
        };
    }
}