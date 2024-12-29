using InventoryManagement.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User,IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<InventoryHistory> InventoryHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Inventory - User ilişkisi
        modelBuilder.Entity<Inventory>()
            .HasOne(i => i.AssignedUser)
            .WithMany()
            .HasForeignKey(i => i.AssignedUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Inventory - Company ilişkisi
        modelBuilder.Entity<Inventory>()
            .HasOne(i => i.SupportCompany)
            .WithMany()
            .HasForeignKey(i => i.SupportCompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        // Ticket - User ilişkisi
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ticket - Inventory ilişkisi
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Inventory)
            .WithMany()
            .HasForeignKey(t => t.InventoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // InventoryHistory ilişkileri (mevcut)
        modelBuilder.Entity<InventoryHistory>()
            .HasOne(ih => ih.Inventory)
            .WithMany(i => i.InventoryHistory)
            .HasForeignKey(ih => ih.InventoryId);

        modelBuilder.Entity<InventoryHistory>()
            .HasOne(ih => ih.User)
            .WithMany()
            .HasForeignKey(ih => ih.UserId);
    }
}