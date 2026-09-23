using Microsoft.EntityFrameworkCore;
using MiniBooking.Models;

namespace MiniBooking.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> opt) : DbContext(opt)
{
    public DbSet<Org> Orgs => Set<Org>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<CareReminder> CareReminders => Set<CareReminder>();
    public DbSet<BirthdayCare> BirthdayCares => Set<BirthdayCare>();
    public DbSet<Engineer> Engineers => Set<Engineer>();
    public DbSet<ServiceBay> ServiceBays => Set<ServiceBay>();
    public DbSet<AppType> AppTypes => Set<AppType>();
    public DbSet<CavityType> CavityTypes => Set<CavityType>();
    public DbSet<CalendarDay> CalendarDays => Set<CalendarDay>();
    public DbSet<AppServiceItem> AppServiceItems => Set<AppServiceItem>();
    public DbSet<AppPartItem> AppPartItems => Set<AppPartItem>();
    public DbSet<RepairOrder> RepairOrders => Set<RepairOrder>();
    public DbSet<RepairOrderServiceItem> RepairOrderServiceItems => Set<RepairOrderServiceItem>();
    public DbSet<RepairOrderPartItem> RepairOrderPartItems => Set<RepairOrderPartItem>();
    public DbSet<RepairOrderStatusHistory> RepairOrderStatusHistories => Set<RepairOrderStatusHistory>();
    public DbSet<RepairOrderDeliveryPlan> RepairOrderDeliveryPlans => Set<RepairOrderDeliveryPlan>();
    public DbSet<PostServiceCare> PostServiceCares => Set<PostServiceCare>();
    public DbSet<PostServiceCare24h> PostServiceCares24h => Set<PostServiceCare24h>();
    public DbSet<ReceptionForm> ReceptionForms => Set<ReceptionForm>();
    public DbSet<WorkAssignment> WorkAssignments => Set<WorkAssignment>();
    public DbSet<WorkAssignmentStage> WorkAssignmentStages => Set<WorkAssignmentStage>();
    public DbSet<WorkAssignmentEngineer> WorkAssignmentEngineers => Set<WorkAssignmentEngineer>();
    public DbSet<ServicePackage> ServicePackages => Set<ServicePackage>();
    public DbSet<ServicePackageServiceItem> ServicePackageServiceItems => Set<ServicePackageServiceItem>();
    public DbSet<ServicePackagePartItem> ServicePackagePartItems => Set<ServicePackagePartItem>();
    public DbSet<CampaignMarketing> CampaignMarketings => Set<CampaignMarketing>();
    public DbSet<CampaignMarketingPart> CampaignMarketingParts => Set<CampaignMarketingPart>();
    public DbSet<CampaignMarketingCondition> CampaignMarketingConditions => Set<CampaignMarketingCondition>();
    public DbSet<MaintenanceSetting> MaintenanceSettings => Set<MaintenanceSetting>();
    public DbSet<ApptStatusHistory> ApptStatusHistories => Set<ApptStatusHistory>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Org>().HasIndex(x => x.ApiKey).IsUnique();
        b.Entity<Appointment>().HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
        b.Entity<Appointment>().Property(x => x.Status).HasConversion<int>();
        b.Entity<BirthdayCare>().HasIndex(x => new { x.OrgId, x.CareBthId }).IsUnique();
        b.Entity<ServiceBay>().HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
        b.Entity<AppType>().HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
        b.Entity<CavityType>().HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
        b.Entity<CalendarDay>().HasIndex(x => new { x.OrgId, x.CalendarType, x.Date }).IsUnique();
        b.Entity<AppServiceItem>().HasIndex(x => new { x.OrgId, x.AppCode, x.SerCode });
        b.Entity<AppPartItem>().HasIndex(x => new { x.OrgId, x.AppCode, x.PartCode });
        b.Entity<RepairOrder>().HasIndex(x => new { x.OrgId, x.RoId }).IsUnique();
        b.Entity<RepairOrderServiceItem>().HasIndex(x => new { x.OrgId, x.RoId, x.SerCode });
        b.Entity<RepairOrderPartItem>().HasIndex(x => new { x.OrgId, x.RoId, x.PartCode });
        b.Entity<RepairOrderStatusHistory>().HasIndex(x => new { x.OrgId, x.RoId, x.ChangedAt });
        b.Entity<RepairOrderDeliveryPlan>().HasIndex(x => new { x.OrgId, x.RoId, x.FlagCurrent });
        b.Entity<PostServiceCare>().HasIndex(x => new { x.OrgId, x.CusCareId }).IsUnique();
        b.Entity<PostServiceCare24h>().HasIndex(x => new { x.OrgId, x.CusCareId }).IsUnique();
        b.Entity<ReceptionForm>().HasIndex(x => new { x.OrgId, x.ReceptionFNo }).IsUnique();
        b.Entity<WorkAssignment>().HasIndex(x => new { x.OrgId, x.RoId }).IsUnique();
        b.Entity<WorkAssignmentStage>().HasIndex(x => new { x.OrgId, x.AssignmentId, x.WorkType });
        b.Entity<WorkAssignmentEngineer>().HasIndex(x => new { x.OrgId, x.AssignmentId, x.EngineerCode, x.WorkType });
        b.Entity<ServicePackage>().HasIndex(x => new { x.OrgId, x.DealerCode, x.PackageNo }).IsUnique();
        b.Entity<ServicePackageServiceItem>().HasIndex(x => new { x.OrgId, x.PackageId, x.SerCode });
        b.Entity<ServicePackagePartItem>().HasIndex(x => new { x.OrgId, x.PackageId, x.PartCode });
        b.Entity<CampaignMarketing>().HasIndex(x => new { x.OrgId, x.CamMarketingNo }).IsUnique();
        b.Entity<CampaignMarketingPart>().HasIndex(x => new { x.OrgId, x.CamMarketingNo, x.PartCode });
        b.Entity<CampaignMarketingCondition>().HasIndex(x => new { x.OrgId, x.CamMarketingNo, x.ConditionType });
        b.Entity<MaintenanceSetting>().HasIndex(x => new { x.OrgId, x.RomsId }).IsUnique();
        b.Entity<MaintenanceSetting>().HasIndex(x => new { x.OrgId, x.Km }).IsUnique();
        b.Entity<ApptStatusHistory>().HasIndex(x => new { x.OrgId, x.AppCode, x.ChangedAt });
    }
}
