using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;

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
    public DbSet<Department> Departments { get; set; }
    public DbSet<DepartmentPermission> DepartmentPermissions { get; set; }
    public DbSet<ProblemType> ProblemTypes { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<SolutionType> SolutionTypes { get; set; }
    public DbSet<TicketSolution> TicketSolutions { get; set; }
    public DbSet<TicketHistory> TicketHistories { get; set; }
    public DbSet<SolutionTime> SolutionTimes { get; set; }
    public DbSet<CancelledTicket> CancelledTickets { get; set; }
    public DbSet<CancelReason> CancelReasons { get; set; }
    public DbSet<PendingReason> PendingReasons { get; set; }
    public DbSet<PendingTicket> PendingTickets { get; set; }
    public DbSet<SolutionReview> SolutionReviews { get; set; }
    public DbSet<UsersAssignedTickets> UsersAssignedTickets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        //group departman ilişkisi
        modelBuilder.Entity<Group>()
            .HasOne(g => g.Department)
            .WithMany(d => d.Groups)
            .HasForeignKey(g => g.DepartmentId);
        
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
        
        modelBuilder.Entity<TicketSolution>()
        .HasOne(ts => ts.Ticket)
        .WithMany()
        .HasForeignKey(ts => ts.TicketId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<TicketSolution>()
        .HasOne(ts => ts.User)
        .WithMany()
        .HasForeignKey(ts => ts.UserId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<TicketSolution>()
        .HasOne(ts => ts.AssignedUser)
        .WithMany()
        .HasForeignKey(ts => ts.AssignedUserId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<TicketSolution>()
        .HasOne(ts => ts.SolutionType)
        .WithMany()
        .HasForeignKey(ts => ts.SolutionTypeId)
        .OnDelete(DeleteBehavior.Restrict);

    // TicketHistory relationships
    modelBuilder.Entity<TicketHistory>()
        .HasOne(th => th.Ticket)
        .WithMany()
        .HasForeignKey(th => th.TicketId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<TicketHistory>()
        .HasOne(th => th.User)
        .WithMany()
        .HasForeignKey(th => th.UserId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<TicketHistory>()
        .HasOne(th => th.FromAssignedUser)
        .WithMany()
        .HasForeignKey(th => th.FromAssignedUserId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<TicketHistory>()
        .HasOne(th => th.ToUser)
        .WithMany()
        .HasForeignKey(th => th.ToUserId)
        .OnDelete(DeleteBehavior.Restrict);

    // SolutionTime relationships
    modelBuilder.Entity<SolutionTime>()
        .HasOne(st => st.ProblemType)
        .WithMany()
        .HasForeignKey(st => st.ProblemTypeId)
        .OnDelete(DeleteBehavior.Restrict);

    // CancelledTicket relationships
    modelBuilder.Entity<CancelledTicket>()
        .HasOne(ct => ct.Ticket)
        .WithMany()
        .HasForeignKey(ct => ct.TicketId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<CancelledTicket>()
        .HasOne(ct => ct.User)
        .WithMany()
        .HasForeignKey(ct => ct.UserId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<CancelledTicket>()
        .HasOne(ct => ct.CancelReason)
        .WithMany()
        .HasForeignKey(ct => ct.CancelReasonId)
        .OnDelete(DeleteBehavior.Restrict);

    // PendingTicket relationships
    modelBuilder.Entity<PendingTicket>()
        .HasOne(pt => pt.Ticket)
        .WithMany()
        .HasForeignKey(pt => pt.TicketId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<PendingTicket>()
        .HasOne(pt => pt.User)
        .WithMany()
        .HasForeignKey(pt => pt.UserId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<PendingTicket>()
        .HasOne(pt => pt.PendingReason)
        .WithMany()
        .HasForeignKey(pt => pt.PendingReasonId)
        .OnDelete(DeleteBehavior.Restrict);
    
    modelBuilder.Entity<Ticket>()
        .Property(t => t.IdleDuration)
        .HasConversion(
            v => v.HasValue ? v.Value.Ticks : (long?)null,
            v => v.HasValue ? TimeSpan.FromTicks(v.Value) : null
        )
        .HasColumnType("bigint");

    }
}