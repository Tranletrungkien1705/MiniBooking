using Microsoft.EntityFrameworkCore;
using MiniBooking.Models;

namespace MiniBooking.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> opt) : DbContext(opt)
{
    public DbSet<Org> Orgs => Set<Org>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<CareReminder> CareReminders => Set<CareReminder>();
    public DbSet<Engineer> Engineers => Set<Engineer>();
    public DbSet<ServiceBay> ServiceBays => Set<ServiceBay>();
    public DbSet<AppType> AppTypes => Set<AppType>();
    public DbSet<CavityType> CavityTypes => Set<CavityType>();
    public DbSet<AppServiceItem> AppServiceItems => Set<AppServiceItem>();
    public DbSet<AppPartItem> AppPartItems => Set<AppPartItem>();
    public DbSet<RepairOrder> RepairOrders => Set<RepairOrder>();
    public DbSet<PostServiceCare> PostServiceCares => Set<PostServiceCare>();
    public DbSet<ReceptionForm> ReceptionForms => Set<ReceptionForm>();
    public DbSet<WorkAssignment> WorkAssignments => Set<WorkAssignment>();
    public DbSet<WorkAssignmentStage> WorkAssignmentStages => Set<WorkAssignmentStage>();
    public DbSet<WorkAssignmentEngineer> WorkAssignmentEngineers => Set<WorkAssignmentEngineer>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Org>().HasIndex(x => x.ApiKey).IsUnique();
        b.Entity<Appointment>().HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
        b.Entity<Appointment>().Property(x => x.Status).HasConversion<int>();
        b.Entity<ServiceBay>().HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
        b.Entity<AppType>().HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
        b.Entity<CavityType>().HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
        b.Entity<AppServiceItem>().HasIndex(x => new { x.OrgId, x.AppCode, x.SerCode });
        b.Entity<AppPartItem>().HasIndex(x => new { x.OrgId, x.AppCode, x.PartCode });
        b.Entity<RepairOrder>().HasIndex(x => new { x.OrgId, x.RoId }).IsUnique();
        b.Entity<PostServiceCare>().HasIndex(x => new { x.OrgId, x.CusCareId }).IsUnique();
        b.Entity<ReceptionForm>().HasIndex(x => new { x.OrgId, x.ReceptionFNo }).IsUnique();
        b.Entity<WorkAssignment>().HasIndex(x => new { x.OrgId, x.RoId }).IsUnique();
        b.Entity<WorkAssignmentStage>().HasIndex(x => new { x.OrgId, x.AssignmentId, x.WorkType });
        b.Entity<WorkAssignmentEngineer>().HasIndex(x => new { x.OrgId, x.AssignmentId, x.EngineerCode, x.WorkType });
    }
}
