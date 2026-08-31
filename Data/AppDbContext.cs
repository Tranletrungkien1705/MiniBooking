using Microsoft.EntityFrameworkCore;
using MiniBooking.Models;

namespace MiniBooking.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> opt) : DbContext(opt)
{
    public DbSet<Org> Orgs => Set<Org>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<CareReminder> CareReminders => Set<CareReminder>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Org>().HasIndex(x => x.ApiKey).IsUnique();
        b.Entity<Appointment>().HasIndex(x => new { x.OrgId, x.Code }).IsUnique();
        b.Entity<Appointment>().Property(x => x.Status).HasConversion<int>();
    }
}
