namespace InventoryManagement.Core.Helpers;

public static class TimeSpanFormatter
{
    public static string Format(TimeSpan ts)
    {
        List<string> parts = new List<string>();
        if (ts.Days > 0) parts.Add($"{ts.Days} day{(ts.Days != 1 ? "s" : "")}");
        if (ts.Hours > 0) parts.Add($"{ts.Hours} hour{(ts.Hours != 1 ? "s" : "")}");
        if (ts.Minutes > 0) parts.Add($"{ts.Minutes} minute{(ts.Minutes != 1 ? "s" : "")}");
            
        return parts.Count > 0 
            ? string.Join(", ", parts) 
            : "Less than a minute";
    }
}