using Bams.Domain;
using Bams.Infrastructure.Identity;
using Bams.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bams.Infrastructure;

public sealed class BamsDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public BamsDbContext(DbContextOptions<BamsDbContext> options) : base(options)
    {
    }
    public DbSet<BranchSchedule> BranchSchedules => Set<BranchSchedule>();

    public DbSet<BranchClosure> BranchClosures => Set<BranchClosure>();

    public DbSet<BookingRequest> BookingRequests => Set<BookingRequest>();

    public DbSet<Branch> Branches => Set<Branch>();

    public DbSet<Service> Services => Set<Service>();

    public DbSet<BranchService> BranchServices => Set<BranchService>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Appointment> Appointments => Set<Appointment>();

    public DbSet<NotificationRecord> Notifications => Set<NotificationRecord>();

    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    public DbSet<RefreshSession> RefreshSessions => Set<RefreshSession>();

    public DbSet<BranchMembership> BranchMemberships => Set<BranchMembership>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BamsDbContext).Assembly);
    }
}
