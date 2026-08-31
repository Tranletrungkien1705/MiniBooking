using Microsoft.EntityFrameworkCore;
using MiniBooking.Models;

namespace MiniBooking.Data;

public static class Seeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        if (!await db.Orgs.AnyAsync(o => o.Id == TenantContext.DefaultOrgId))
        {
            db.Orgs.Add(new Org { Id = TenantContext.DefaultOrgId, Name = "Demo Service Center", ApiKey = "demo-booking" });
            await db.SaveChangesAsync();
        }
    }
}
