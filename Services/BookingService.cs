using Microsoft.EntityFrameworkCore;
using MiniBooking.Data;
using MiniBooking.Models;

namespace MiniBooking.Services;

public record BookDto(string CustomerName, string Phone, string? Vin, string? Plate, string? ServiceType, DateTime PreferredAt, string? DealerCode, string? Note);
public record ConfirmDto(string? Engineer);
public record CheckInDto(string? RoNo);

public interface IBookingService
{
    Task<object> BookAsync(BookDto dto);        // công khai (khách)
    Task<object?> StatusAsync(string code);     // công khai
    Task<object> ListAsync(string? status, string? dealer, string? date);
    Task<object?> ConfirmAsync(string code, string? engineer);
    Task<object?> CheckInAsync(string code, string? roNo);
    Task<object?> DoneAsync(string code);
    Task<object?> CancelAsync(string code, bool noShow);
    Task<object> CalendarAsync(string date, string? dealer);
    Task<object> StatsAsync();
}

public sealed class BookingService(AppDbContext db, ITenantContext tenant) : IBookingService
{
    private Guid Org => tenant.OrgId;
    private static readonly (ApptStatus,string)[] Steps = { (ApptStatus.Requested,"Chờ xác nhận"),(ApptStatus.Confirmed,"Đã xác nhận"),(ApptStatus.CheckedIn,"Đã tiếp nhận"),(ApptStatus.Done,"Hoàn tất"),(ApptStatus.Cancelled,"Đã hủy"),(ApptStatus.NoShow,"Không đến") };
    private static string Text(ApptStatus s) => Steps.First(x => x.Item1 == s).Item2;

    public async Task<object> BookAsync(BookDto dto)
    {
        var code = "AP" + DateTime.Now.ToString("yyMMddHHmmss") + Random.Shared.Next(10, 99);
        var a = new Appointment
        {
            OrgId = Org, Code = code, CustomerName = dto.CustomerName.Trim(), Phone = dto.Phone.Trim(),
            Vin = dto.Vin?.Trim().ToUpperInvariant(), Plate = dto.Plate?.Trim(),
            ServiceType = string.IsNullOrWhiteSpace(dto.ServiceType) ? "Bảo dưỡng" : dto.ServiceType!.Trim(),
            PreferredAt = dto.PreferredAt, DealerCode = dto.DealerCode?.Trim() ?? "", Note = dto.Note,
            Status = ApptStatus.Requested
        };
        db.Appointments.Add(a);
        await db.SaveChangesAsync();
        return new { a.Code, status = a.Status.ToString(), statusText = Text(a.Status), a.PreferredAt };
    }

    public async Task<object?> StatusAsync(string code)
    {
        code = code.Trim().ToUpperInvariant();
        var a = await db.Appointments.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Code == code);
        if (a is null) return null;
        return new { a.Code, a.CustomerName, a.ServiceType, a.PreferredAt, status = a.Status.ToString(), statusText = Text(a.Status), a.Engineer, a.RoNo };
    }

    public async Task<object> ListAsync(string? status, string? dealer, string? date)
    {
        var q = db.Appointments.Where(a => a.OrgId == Org);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ApptStatus>(status, true, out var st)) q = q.Where(a => a.Status == st);
        if (!string.IsNullOrWhiteSpace(dealer)) q = q.Where(a => a.DealerCode == dealer);
        if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var d)) q = q.Where(a => a.PreferredAt.Date == d.Date);
        var items = await q.OrderBy(a => a.PreferredAt).Take(500).Select(a => new
        {
            a.Code, a.CustomerName, a.Phone, a.Vin, a.Plate, a.ServiceType, a.PreferredAt,
            a.DealerCode, a.Engineer, status = a.Status.ToString(), statusText = Text(a.Status), a.RoNo
        }).ToListAsync();
        return new { count = items.Count, items };
    }

    private async Task<Appointment?> Get(string code)
    {
        code = code.Trim().ToUpperInvariant();
        return await db.Appointments.FirstOrDefaultAsync(x => x.OrgId == Org && x.Code == code);
    }

    public async Task<object?> ConfirmAsync(string code, string? engineer)
    {
        var a = await Get(code);
        if (a is null || a.Status != ApptStatus.Requested) return null;
        a.Status = ApptStatus.Confirmed; a.Engineer = engineer;
        await db.SaveChangesAsync();
        return new { a.Code, status = a.Status.ToString(), a.Engineer };
    }

    public async Task<object?> CheckInAsync(string code, string? roNo)
    {
        var a = await Get(code);
        if (a is null || a.Status is not (ApptStatus.Confirmed or ApptStatus.Requested)) return null;
        a.Status = ApptStatus.CheckedIn; a.CheckedInAt = DateTime.Now;
        a.RoNo = string.IsNullOrWhiteSpace(roNo) ? "RO" + DateTime.Now.ToString("yyMMddHHmmss") : roNo!.Trim();
        await db.SaveChangesAsync();
        return new { a.Code, status = a.Status.ToString(), a.RoNo, a.CheckedInAt };   // RoNo dùng nối sang MiniService
    }

    public async Task<object?> DoneAsync(string code)
    {
        var a = await Get(code);
        if (a is null || a.Status != ApptStatus.CheckedIn) return null;
        a.Status = ApptStatus.Done; a.DoneAt = DateTime.Now;
        await db.SaveChangesAsync();
        return new { a.Code, status = a.Status.ToString(), a.DoneAt };
    }

    public async Task<object?> CancelAsync(string code, bool noShow)
    {
        var a = await Get(code);
        if (a is null || a.Status is ApptStatus.Done or ApptStatus.Cancelled) return null;
        a.Status = noShow ? ApptStatus.NoShow : ApptStatus.Cancelled;
        await db.SaveChangesAsync();
        return new { a.Code, status = a.Status.ToString() };
    }

    public async Task<object> CalendarAsync(string date, string? dealer)
    {
        DateTime.TryParse(date, out var d);
        var q = db.Appointments.Where(a => a.OrgId == Org && a.PreferredAt.Date == d.Date);
        if (!string.IsNullOrWhiteSpace(dealer)) q = q.Where(a => a.DealerCode == dealer);
        var slots = await q.OrderBy(a => a.PreferredAt).Select(a => new
        {
            time = a.PreferredAt, a.Code, a.CustomerName, a.ServiceType, a.Engineer, status = a.Status.ToString()
        }).ToListAsync();
        return new { date = d.ToString("yyyy-MM-dd"), count = slots.Count, slots };
    }

    public async Task<object> StatsAsync()
    {
        var q = db.Appointments.Where(a => a.OrgId == Org);
        var byStatus = await q.GroupBy(a => a.Status).Select(g => new { s = g.Key, c = g.Count() }).ToListAsync();
        return new
        {
            total = await q.CountAsync(),
            requested = byStatus.FirstOrDefault(x => x.s == ApptStatus.Requested)?.c ?? 0,
            confirmed = byStatus.FirstOrDefault(x => x.s == ApptStatus.Confirmed)?.c ?? 0,
            checkedIn = byStatus.FirstOrDefault(x => x.s == ApptStatus.CheckedIn)?.c ?? 0,
            done = byStatus.FirstOrDefault(x => x.s == ApptStatus.Done)?.c ?? 0,
            cancelled = byStatus.FirstOrDefault(x => x.s == ApptStatus.Cancelled)?.c ?? 0,
            noShow = byStatus.FirstOrDefault(x => x.s == ApptStatus.NoShow)?.c ?? 0
        };
    }
}
