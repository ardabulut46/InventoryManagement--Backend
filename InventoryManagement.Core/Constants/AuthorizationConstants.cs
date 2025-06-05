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
        public const string UploadInvoice = "Inventory:UploadFile";
        public const string ViewFiles = "Inventory:ViewFiles";
        public const string ViewAssignmentHistory = "Inventory:ViewAssignmentHistory";
        public const string ViewPurchaseInfo = "Inventory:ViewPurchaseInfo";
        public const string ViewAssignmentDocuments = "Inventory:ViewAssignmentDocuments";
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
        public const string ViewWhoCreated = "Tickets:ViewWhoCreated";
    }
    //Role permissions
    public static class Roles
    {   
        public const string View = "Roles:View";
        public const string Create = "Roles:Create";
        public const string Edit = "Roles:Edit";
        public const string Delete = "Roles:Delete";
    }

    //Reports permissions
    public static class Reports
    {
        public const string View = "Reports:View";
    }

    // Access to admin panel
    public static class AdminPanel
    {
        public const string View = "AdminPanel:View";
    }
    
    
}
    
public static class Policies
{
    // Role-based policies
    public const string InventoryManager = "InventoryManager";
    public const string UserManager = "UserManager";
    public const string SuperAdmin = "SuperAdmin";
}