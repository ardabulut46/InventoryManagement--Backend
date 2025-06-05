using InventoryManagement.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManagement.API.Extensions
{
    public static class AuthorizationPolicyExtensions
    {
        public static IServiceCollection AddApplicationAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Basic Policies
                options.AddPolicy(Permissions.CanView, policy =>
                    policy.RequireClaim("Permission", Permissions.CanView));
                options.AddPolicy(Permissions.CanCreate, policy =>
                    policy.RequireClaim("Permission", Permissions.CanCreate));
                options.AddPolicy(Permissions.CanEdit, policy =>
                    policy.RequireClaim("Permission", Permissions.CanEdit));
                options.AddPolicy(Permissions.CanDelete, policy =>
                    policy.RequireClaim("Permission", Permissions.CanDelete));
                
                // Inventory Policies
                options.AddPolicy(Permissions.Inventory.View, policy =>
                    policy.RequireClaim("Permission", Permissions.Inventory.View));
                options.AddPolicy(Permissions.Inventory.Create, policy =>
                    policy.RequireClaim("Permission", Permissions.Inventory.Create));
                options.AddPolicy(Permissions.Inventory.Edit, policy =>
                    policy.RequireClaim("Permission", Permissions.Inventory.Edit));
                options.AddPolicy(Permissions.Inventory.Delete, policy =>
                    policy.RequireClaim("Permission", Permissions.Inventory.Delete));
                options.AddPolicy(Permissions.Inventory.UploadInvoice, policy =>
                    policy.RequireClaim("Permission", Permissions.Inventory.UploadInvoice));
                options.AddPolicy(Permissions.Inventory.ViewFiles, policy =>
                    policy.RequireClaim("Permission", Permissions.Inventory.ViewFiles));
                options.AddPolicy(Permissions.Inventory.ViewAssignmentHistory, policy =>
                    policy.RequireClaim("Permission", Permissions.Inventory.ViewAssignmentHistory));
                options.AddPolicy(Permissions.Inventory.ViewPurchaseInfo, policy =>
                    policy.RequireClaim("Permission", Permissions.Inventory.ViewPurchaseInfo));
                options.AddPolicy(Permissions.Inventory.ViewAssignmentDocuments, policy =>
                    policy.RequireClaim("Permission", Permissions.Inventory.ViewAssignmentDocuments));
                
                
                
                
                // User Policies
                options.AddPolicy(Permissions.Users.View, policy =>
                    policy.RequireClaim("Permission", Permissions.Users.View));
                options.AddPolicy(Permissions.Users.Create, policy =>
                    policy.RequireClaim("Permission", Permissions.Users.Create));
                options.AddPolicy(Permissions.Users.Edit, policy =>
                    policy.RequireClaim("Permission", Permissions.Users.Edit));
                options.AddPolicy(Permissions.Users.Delete, policy =>
                    policy.RequireClaim("Permission", Permissions.Users.Delete));
                
                // Ticket Policies
                options.AddPolicy(Permissions.Tickets.View, policy =>
                    policy.RequireClaim("Permission", Permissions.Tickets.View));
                options.AddPolicy(Permissions.Tickets.Create, policy =>
                    policy.RequireClaim("Permission", Permissions.Tickets.Create));
                options.AddPolicy(Permissions.Tickets.Edit, policy =>
                    policy.RequireClaim("Permission", Permissions.Tickets.Edit));
                options.AddPolicy(Permissions.Tickets.Delete, policy =>
                    policy.RequireClaim("Permission", Permissions.Tickets.Delete));
                options.AddPolicy(Permissions.Tickets.Assign, policy =>
                    policy.RequireClaim("Permission", Permissions.Tickets.Assign));
                
                // Role Policies
                options.AddPolicy(Permissions.Roles.View, policy =>
                    policy.RequireClaim("Permission", Permissions.Roles.View));
                options.AddPolicy(Permissions.Roles.Create, policy =>
                    policy.RequireClaim("Permission", Permissions.Roles.Create));
                options.AddPolicy(Permissions.Roles.Delete, policy =>
                    policy.RequireClaim("Permission", Permissions.Roles.Delete));
                options.AddPolicy(Permissions.Roles.Edit, policy =>
                    policy.RequireClaim("Permission", Permissions.Roles.Edit));
                
                // Access to admin panel
                options.AddPolicy(Permissions.AdminPanel.View, policy =>
                    policy.RequireClaim("Permission", Permissions.AdminPanel.View));
                
                
                // Role-based composite policies
                options.AddPolicy(Policies.InventoryManager, policy =>
                    policy.RequireAssertion(context => 
                        context.User.HasClaim(c => 
                            c.Type == "Permission" && 
                            (c.Value == Permissions.Inventory.View || 
                             c.Value == Permissions.Inventory.Create || 
                             c.Value == Permissions.Inventory.Edit))));
                
                options.AddPolicy(Policies.UserManager, policy =>
                    policy.RequireAssertion(context => 
                        context.User.HasClaim(c => 
                            c.Type == "Permission" && 
                            (c.Value == Permissions.Users.View || 
                             c.Value == Permissions.Users.Create || 
                             c.Value == Permissions.Users.Edit))));
                
                options.AddPolicy(Policies.SuperAdmin, policy =>
                    policy.RequireRole("SuperAdmin"));
            });
            
            return services;
        }
    }
}