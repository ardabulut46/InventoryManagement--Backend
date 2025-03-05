namespace InventoryManagement.Core.Constants;

public static class Permissions
{
    // Basic Permissions
    public const string CanView = "CanView";
    public const string CanCreate = "CanCreate";
    public const string CanEdit = "CanEdit";
    public const string CanDelete = "CanDelete";
        
    // Inventory Permissions
    public static class Inventory
    {
        public const string View = "Inventory:View";
        public const string Create = "Inventory:Create";
        public const string Edit = "Inventory:Edit";
        public const string Delete = "Inventory:Delete";
    }
        
    // User Permissions
    public static class Users
    {
        public const string View = "Users:View";
        public const string Create = "Users:Create";
        public const string Edit = "Users:Edit";
        public const string Delete = "Users:Delete";
    }
        
    // Ticket Permissions
    public static class Tickets
    {
        public const string View = "Tickets:View";
        public const string Create = "Tickets:Create";
        public const string Edit = "Tickets:Edit";
        public const string Delete = "Tickets:Delete";
        public const string Assign = "Tickets:Assign";
    }
}
    
public static class Policies
{
    // Role-based policies
    public const string InventoryManager = "InventoryManager";
    public const string UserManager = "UserManager";
    public const string SuperAdmin = "SuperAdmin";
}