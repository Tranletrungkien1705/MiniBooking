using Microsoft.EntityFrameworkCore;
using MiniBooking.Data;
using MiniBooking.Models;

namespace MiniBooking.Services;

public record BookDto(string CustomerName, string Phone, string? Vin, string? Plate, string? ServiceType, DateTime PreferredAt, string? DealerCode, string? Note, string? BayCode = null, string? AppTypeCode = null, DateTime? SlotTo = null);
public record ConfirmDto(string? Engineer, string? BayCode = null, DateTime? SlotFrom = null, DateTime? SlotTo = null);
public record ContactApptDto(string? Result, string? Note);   // Result: Confirmed/NoAnswer/Rejected
public record CheckInDto(string? RoNo);
public record CreateReminderDto(string CustomerName, string Phone, string? Vin, string? Plate, string? CareType, DateTime DueDate, string? Note);
public record ContactDto(string? Note);
public record ConvertDto(DateTime PreferredAt, string? ServiceType, string? DealerCode);
public record AddEngineerDto(string Code, string Name, string? Skill, string? DealerCode);
public record AddBayDto(string Code, string Name, string? BayType, int? CapacityPerSlot, string? DealerCode, string? Note);
public record AddAppTypeDto(string Code, string Name);
public record AddCavityTypeDto(string Code, string Name);
public record SlotQueryDto(string Date, string? BayCode, string? DealerCode);

public interface IBookingService
{
    Task<object> BookAsync(BookDto dto);        // công khai (khách)
    Task<object?> StatusAsync(string code);     // công khai
    Task<object> ListAsync(string? status, string? dealer, string? date);
    Task<object?> ContactAsync(string code, ContactApptDto dto);   // gọi xác nhận trước giờ hẹn (AppStatus=5)
    Task<object> DueForContactAsync(int withinHours);              // danh sách lịch cần gọi xác nhận
    Task<object?> ConfirmAsync(string code, ConfirmDto dto);
    Task<object?> CheckInAsync(string code, string? roNo);
    Task<object?> DoneAsync(string code);
    Task<object?> CancelAsync(string code, bool noShow);
    Task<object> CalendarAsync(string date, string? dealer);
    Task<object> StatsAsync();
    Task<object> CreateReminderAsync(CreateReminderDto dto);
    Task<object> ListRemindersAsync(string? status, string? careType, string? dueBefore);
    Task<object?> ContactReminderAsync(long id, string? note);
    Task<object?> ConvertReminderAsync(long id, ConvertDto dto);
    Task<object> CareStatsAsync();
    Task<object> AddEngineerAsync(AddEngineerDto dto);
    Task<object> EngineerWorkloadAsync(string date, string? dealer);
    Task<object> AddBayAsync(AddBayDto dto);
    Task<object> ListBaysAsync(string? dealer);
    Task<object> SlotAvailabilityAsync(string date, string? bayCode, string? dealer);
    Task<object> AddAppTypeAsync(AddAppTypeDto dto);
    Task<object> ListAppTypesAsync(bool? active);
    Task<object> AddCavityTypeAsync(AddCavityTypeDto dto);
    Task<object> ListCavityTypesAsync(bool? active);
}

public sealed class BookingService(AppDbContext db, ITenantContext tenant) : IBookingService
{
    private Guid Org => tenant.OrgId;
    private static readonly (ApptStatus,string)[] Steps = { (ApptStatus.Requested,"Chờ xác nhận"),(ApptStatus.Contacted,"Đã liên hệ & Chưa xác nhận"),(ApptStatus.Confirmed,"Đã xác nhận"),(ApptStatus.CheckedIn,"Đã tiếp nhận"),(ApptStatus.Done,"Hoàn tất"),(ApptStatus.Cancelled,"Đã hủy"),(ApptStatus.NoShow,"Không đến") };
    private static string Text(ApptStatus s) => Steps.First(x => x.Item1 == s).Item2;

    public async Task<object> BookAsync(BookDto dto)
    {
        var code = "AP" + DateTime.Now.ToString("yyMMddHHmmss") + Random.Shared.Next(10, 99);
        var serviceType = string.IsNullOrWhiteSpace(dto.ServiceType) ? "Bảo dưỡng" : dto.ServiceType!.Trim();

        // SerAppCreateDL_AppTypeCodeNotEmpty: loại cuộc hẹn phải có và tồn tại trong master Mst_Ser_AppType.
        var appTypeCode = dto.AppTypeCode?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(appTypeCode))
        {
            var ok = await db.AppTypes.AnyAsync(x => x.OrgId == Org && x.Code == appTypeCode && x.Active);
            if (!ok) throw new InvalidOperationException($"Loại cuộc hẹn '{appTypeCode}' không tồn tại hoặc đã ngừng dùng.");
        }

        var bayCode = dto.BayCode?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(bayCode))
        {
            var bay = await db.ServiceBays.FirstOrDefaultAsync(x => x.OrgId == Org && x.Code == bayCode && x.Active);
            if (bay is null) throw new InvalidOperationException($"Khoang '{bayCode}' không tồn tại hoặc đã ngừng dùng.");
            // Ràng buộc khoang theo loại dịch vụ (Ser_Cavity.CavityType ↔ Ser_App.ServiceType).
            if (!CavityRules.IsCompatible(serviceType, bay.BayType))
                throw new InvalidOperationException($"Khoang '{bayCode}' (loại {bay.BayType}) không phù hợp với dịch vụ '{serviceType}'.");
            var booked = await db.Appointments.CountAsync(a => a.OrgId == Org && a.BayCode == bayCode
                && a.PreferredAt.Date == dto.PreferredAt.Date
                && a.Status != ApptStatus.Cancelled && a.Status != ApptStatus.NoShow);
            if (booked >= bay.CapacityPerSlot)
                throw new InvalidOperationException($"Khoang '{bayCode}' đã đầy trong ngày {dto.PreferredAt:yyyy-MM-dd} ({booked}/{bay.CapacityPerSlot}).");
        }
        var a = new Appointment
        {
            OrgId = Org, Code = code, CustomerName = dto.CustomerName.Trim(), Phone = dto.Phone.Trim(),
            Vin = dto.Vin?.Trim().ToUpperInvariant(), Plate = dto.Plate?.Trim(),
            ServiceType = serviceType,
            PreferredAt = dto.PreferredAt, DealerCode = dto.DealerCode?.Trim() ?? "", Note = dto.Note,
            BayCode = bayCode, AppTypeCode = appTypeCode,
            Status = ApptStatus.Requested
        };
        db.Appointments.Add(a);
        await db.SaveChangesAsync();
        return new { a.Code, status = a.Status.ToString(), statusText = Text(a.Status), a.PreferredAt, a.BayCode, a.AppTypeCode };
    }

    public async Task<object?> StatusAsync(string code)
    {
        code = code.Trim().ToUpperInvariant();
        var a = await db.Appointments.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Code == code);
        if (a is null) return null;
        return new { a.Code, a.CustomerName, a.ServiceType, a.PreferredAt, status = a.Status.ToString(), statusText = Text(a.Status), a.Engineer, a.RoNo, a.BayCode, a.AppTypeCode, a.ContactedAt, a.ContactResult };
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
            a.DealerCode, a.Engineer, status = a.Status.ToString(), statusText = Text(a.Status), a.RoNo, a.BayCode, a.AppTypeCode,
            a.ContactedAt, a.ContactResult, a.ContactNote
        }).ToListAsync();
        return new { count = items.Count, items };
    }

    private async Task<Appointment?> Get(string code)
    {
        code = code.Trim().ToUpperInvariant();
        return await db.Appointments.FirstOrDefaultAsync(x => x.OrgId == Org && x.Code == code);
    }

    // Gọi xác nhận lịch trước giờ hẹn (Ser_CustomerCare72h): Requested → Contacted (AppStatus=5).
    // Kết quả cuộc gọi: Confirmed (CIFB) / NoAnswer (CINFB) / Rejected (REJ).
    public async Task<object?> ContactAsync(string code, ContactApptDto dto)
    {
        var a = await Get(code);
        if (a is null || a.Status != ApptStatus.Requested) return null;
        var result = string.IsNullOrWhiteSpace(dto.Result) ? "NoAnswer" : dto.Result!.Trim();
        a.Status = ApptStatus.Contacted;
        a.ContactedAt = DateTime.Now;
        a.ContactResult = result;
        if (!string.IsNullOrWhiteSpace(dto.Note)) a.ContactNote = dto.Note;
        await db.SaveChangesAsync();
        return new { a.Code, status = a.Status.ToString(), statusText = Text(a.Status), a.ContactedAt, a.ContactResult, a.ContactNote };
    }

    // Danh sách lịch cần gọi xác nhận: còn ở trạng thái Chờ xác nhận và giờ hẹn trong vòng N giờ tới.
    public async Task<object> DueForContactAsync(int withinHours)
    {
        if (withinHours <= 0) withinHours = 24;
        var now = DateTime.Now;
        var until = now.AddHours(withinHours);
        var items = await db.Appointments
            .Where(a => a.OrgId == Org && a.Status == ApptStatus.Requested
                && a.PreferredAt >= now && a.PreferredAt <= until)
            .OrderBy(a => a.PreferredAt)
            .Select(a => new
            {
                a.Code, a.CustomerName, a.Phone, a.Plate, a.ServiceType, a.PreferredAt,
                a.DealerCode, a.Engineer, status = a.Status.ToString(), statusText = Text(a.Status)
            }).ToListAsync();
        return new { withinHours, from = now, to = until, count = items.Count, items };
    }

    public async Task<object?> ConfirmAsync(string code, ConfirmDto dto)
    {
        var a = await Get(code);
        if (a is null || a.Status is not (ApptStatus.Requested or ApptStatus.Contacted)) return null;

        // Gán khoang + khung giờ khi xác nhận (Ser_App: AppDateTimeFrom/AppTimeFrom → AppDateTime/AppTime).
        var bayCode = (dto.BayCode ?? a.BayCode)?.Trim().ToUpperInvariant();
        var slotFrom = dto.SlotFrom ?? a.PreferredAt;
        var slotTo = dto.SlotTo ?? a.SlotTo ?? slotFrom.AddHours(1);
        if (slotTo <= slotFrom) slotTo = slotFrom.AddHours(1);

        if (!string.IsNullOrWhiteSpace(bayCode))
        {
            var bay = await db.ServiceBays.FirstOrDefaultAsync(x => x.OrgId == Org && x.Code == bayCode && x.Active);
            if (bay is null) throw new InvalidOperationException($"Khoang '{bayCode}' không tồn tại hoặc đã ngừng dùng.");
            // Ràng buộc khoang theo loại dịch vụ (Ser_Cavity.CavityType ↔ Ser_App.ServiceType).
            if (!CavityRules.IsCompatible(a.ServiceType, bay.BayType))
                throw new InvalidOperationException($"Khoang '{bayCode}' (loại {bay.BayType}) không phù hợp với dịch vụ '{a.ServiceType}'.");
            // MyCheck_DateTime_Cavity: chặn trùng khung giờ trên cùng khoang (bỏ qua lịch đã hủy/không đến).
            var conflict = await FindBayConflictAsync(bayCode, slotFrom, slotTo, a.Id);
            if (conflict is not null)
                throw new InvalidOperationException($"Khoang '{bayCode}' đã có lịch {conflict} trùng khung giờ {slotFrom:HH:mm}-{slotTo:HH:mm}.");
        }

        a.Status = ApptStatus.Confirmed; a.Engineer = dto.Engineer;
        a.BayCode = bayCode; a.SlotFrom = slotFrom; a.SlotTo = slotTo;
        a.ContactResult = "Confirmed";   // xác nhận qua gọi điện (Ser_CustomerCare72h CIFB)
        await db.SaveChangesAsync();
        return new { a.Code, status = a.Status.ToString(), a.Engineer, a.BayCode, a.SlotFrom, a.SlotTo };
    }

    // Trả về mô tả lịch trùng (nếu có) trên cùng khoang: dùng công thức giao khung giờ của Ser_App.
    private async Task<string?> FindBayConflictAsync(string bayCode, DateTime from, DateTime to, long excludeId)
    {
        var cands = await db.Appointments.Where(x => x.OrgId == Org && x.BayCode == bayCode && x.Id != excludeId
            && x.Status != ApptStatus.Cancelled && x.Status != ApptStatus.NoShow).ToListAsync();
        foreach (var c in cands)
        {
            var cFrom = c.SlotFrom ?? c.PreferredAt;
            var cTo = c.SlotTo ?? cFrom.AddHours(1);
            if (cFrom < to && cTo > from)   // giao nhau
                return $"{c.Code} ({cFrom:HH:mm}-{cTo:HH:mm})";
        }
        return null;
    }

    public async Task<object?> CheckInAsync(string code, string? roNo)
    {
        var a = await Get(code);
        if (a is null || a.Status is not (ApptStatus.Confirmed or ApptStatus.Requested or ApptStatus.Contacted)) return null;
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
        // Ser_App_UpdateStatusX: không cho Hủy khi lịch đã Tiếp nhận (AppStatus=3).
        if (a.Status == ApptStatus.CheckedIn)
            throw new InvalidOperationException($"Lịch {a.Code} đã tiếp nhận, không thể hủy.");
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
            contacted = byStatus.FirstOrDefault(x => x.s == ApptStatus.Contacted)?.c ?? 0,
            confirmed = byStatus.FirstOrDefault(x => x.s == ApptStatus.Confirmed)?.c ?? 0,
            checkedIn = byStatus.FirstOrDefault(x => x.s == ApptStatus.CheckedIn)?.c ?? 0,
            done = byStatus.FirstOrDefault(x => x.s == ApptStatus.Done)?.c ?? 0,
            cancelled = byStatus.FirstOrDefault(x => x.s == ApptStatus.Cancelled)?.c ?? 0,
            noShow = byStatus.FirstOrDefault(x => x.s == ApptStatus.NoShow)?.c ?? 0
        };
    }

    // ===== Kỹ thuật viên (Ser_Engineer) =====
    public async Task<object> AddEngineerAsync(AddEngineerDto dto)
    {
        var code = dto.Code.Trim().ToUpperInvariant();
        var e = await db.Engineers.FirstOrDefaultAsync(x => x.OrgId == Org && x.Code == code);
        if (e is null) { e = new Engineer { OrgId = Org, Code = code, Name = dto.Name.Trim(), Skill = dto.Skill, DealerCode = dto.DealerCode?.Trim() ?? "", Active = true }; db.Engineers.Add(e); }
        else { e.Name = dto.Name.Trim(); e.Skill = dto.Skill; e.DealerCode = dto.DealerCode?.Trim() ?? e.DealerCode; }
        await db.SaveChangesAsync();
        return new { e.Code, e.Name, e.Skill, e.DealerCode, e.Active };
    }

    // Tải công việc KTV theo ngày: đếm lịch hẹn Confirmed/CheckedIn gán mỗi KTV.
    public async Task<object> EngineerWorkloadAsync(string date, string? dealer)
    {
        DateTime.TryParse(date, out var d);
        var engs = db.Engineers.Where(x => x.OrgId == Org && x.Active);
        if (!string.IsNullOrWhiteSpace(dealer)) engs = engs.Where(x => x.DealerCode == dealer);
        var list = await engs.OrderBy(x => x.Code).ToListAsync();
        var appts = await db.Appointments.Where(a => a.OrgId == Org && a.PreferredAt.Date == d.Date
            && (a.Status == ApptStatus.Confirmed || a.Status == ApptStatus.CheckedIn)).ToListAsync();
        var items = list.Select(e => new
        {
            e.Code, e.Name, e.Skill, e.DealerCode,
            jobs = appts.Count(a => a.Engineer == e.Name || a.Engineer == e.Code)
        });
        return new { date = d.ToString("yyyy-MM-dd"), engineers = list.Count, items };
    }

    // ===== Chăm sóc KH dịch vụ (Ser_CustomerCare) =====
    public async Task<object> CreateReminderAsync(CreateReminderDto dto)
    {
        var r = new CareReminder
        {
            OrgId = Org, CustomerName = dto.CustomerName.Trim(), Phone = dto.Phone.Trim(),
            Vin = dto.Vin?.Trim().ToUpperInvariant(), Plate = dto.Plate?.Trim(),
            CareType = string.IsNullOrWhiteSpace(dto.CareType) ? "Maintenance" : dto.CareType!.Trim(),
            DueDate = dto.DueDate, Note = dto.Note, Status = "Pending"
        };
        db.CareReminders.Add(r);
        await db.SaveChangesAsync();
        return new { r.Id, r.CustomerName, r.CareType, r.DueDate, r.Status };
    }

    public async Task<object> ListRemindersAsync(string? status, string? careType, string? dueBefore)
    {
        var q = db.CareReminders.Where(r => r.OrgId == Org);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(r => r.Status == status);
        if (!string.IsNullOrWhiteSpace(careType)) q = q.Where(r => r.CareType == careType);
        if (!string.IsNullOrWhiteSpace(dueBefore) && DateTime.TryParse(dueBefore, out var d)) q = q.Where(r => r.DueDate.Date <= d.Date);
        var items = await q.OrderBy(r => r.DueDate).Take(500).Select(r => new
        {
            r.Id, r.CustomerName, r.Phone, r.Vin, r.Plate, r.CareType, r.DueDate, r.Status, r.Note, r.BookingCode, r.ContactedAt
        }).ToListAsync();
        return new { count = items.Count, items };
    }

    public async Task<object?> ContactReminderAsync(long id, string? note)
    {
        var r = await db.CareReminders.FirstOrDefaultAsync(x => x.OrgId == Org && x.Id == id);
        if (r is null || r.Status is "Booked" or "Closed") return null;
        r.Status = "Contacted"; r.ContactedAt = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(note)) r.Note = note;
        await db.SaveChangesAsync();
        return new { r.Id, r.Status, r.ContactedAt };
    }

    // Chuyển nhắc CSKH thành lịch hẹn thực (Ser_CustomerCare → Ser_App)
    public async Task<object?> ConvertReminderAsync(long id, ConvertDto dto)
    {
        var r = await db.CareReminders.FirstOrDefaultAsync(x => x.OrgId == Org && x.Id == id);
        if (r is null || r.Status == "Booked") return null;
        var book = (dynamic)await BookAsync(new BookDto(r.CustomerName, r.Phone, r.Vin, r.Plate,
            dto.ServiceType ?? (r.CareType == "Maintenance" ? "Bảo dưỡng định kỳ" : r.CareType),
            dto.PreferredAt, dto.DealerCode, $"Từ nhắc CSKH #{r.Id} ({r.CareType})"));
        string code = book.Code;
        r.Status = "Booked"; r.BookingCode = code;
        await db.SaveChangesAsync();
        return new { r.Id, r.Status, bookingCode = code, dto.PreferredAt };
    }

    public async Task<object> CareStatsAsync()
    {
        var q = db.CareReminders.Where(r => r.OrgId == Org);
        var today = DateTime.Now.Date;
        return new
        {
            total = await q.CountAsync(),
            pending = await q.CountAsync(r => r.Status == "Pending"),
            contacted = await q.CountAsync(r => r.Status == "Contacted"),
            booked = await q.CountAsync(r => r.Status == "Booked"),
            overdue = await q.CountAsync(r => r.Status == "Pending" && r.DueDate.Date < today)
        };
    }

    // ===== Khoang sửa chữa (Ser_Cavity) + sức chứa theo khung giờ =====
    public async Task<object> AddBayAsync(AddBayDto dto)
    {
        var code = dto.Code.Trim().ToUpperInvariant();
        var b = await db.ServiceBays.FirstOrDefaultAsync(x => x.OrgId == Org && x.Code == code);
        if (b is null)
        {
            b = new ServiceBay { OrgId = Org, Code = code, Name = dto.Name.Trim(), BayType = string.IsNullOrWhiteSpace(dto.BayType) ? "General" : dto.BayType!.Trim(), CapacityPerSlot = Math.Max(1, dto.CapacityPerSlot ?? 1), DealerCode = dto.DealerCode?.Trim() ?? "", Note = dto.Note, Active = true };
            db.ServiceBays.Add(b);
        }
        else
        {
            b.Name = dto.Name.Trim();
            b.BayType = string.IsNullOrWhiteSpace(dto.BayType) ? b.BayType : dto.BayType!.Trim();
            b.CapacityPerSlot = Math.Max(1, dto.CapacityPerSlot ?? b.CapacityPerSlot);
            b.DealerCode = dto.DealerCode?.Trim() ?? b.DealerCode;
            if (dto.Note != null) b.Note = dto.Note;
        }
        await db.SaveChangesAsync();
        return new { b.Code, b.Name, b.BayType, b.CapacityPerSlot, b.DealerCode, b.Active };
    }

    public async Task<object> ListBaysAsync(string? dealer)
    {
        var q = db.ServiceBays.Where(x => x.OrgId == Org);
        if (!string.IsNullOrWhiteSpace(dealer)) q = q.Where(x => x.DealerCode == dealer);
        var items = await q.OrderBy(x => x.Code).Select(x => new
        {
            x.Code, x.Name, x.BayType, x.CapacityPerSlot, x.DealerCode, x.Active, x.Note
        }).ToListAsync();
        return new { count = items.Count, items };
    }

    // Sức chứa còn lại theo từng khoang trong 1 ngày: đếm lịch chưa hủy/không đến theo giờ.
    public async Task<object> SlotAvailabilityAsync(string date, string? bayCode, string? dealer)
    {
        DateTime.TryParse(date, out var d);
        var baysQ = db.ServiceBays.Where(x => x.OrgId == Org && x.Active);
        if (!string.IsNullOrWhiteSpace(bayCode)) baysQ = baysQ.Where(x => x.Code == bayCode.ToUpperInvariant());
        if (!string.IsNullOrWhiteSpace(dealer)) baysQ = baysQ.Where(x => x.DealerCode == dealer);
        var bays = await baysQ.OrderBy(x => x.Code).ToListAsync();

        var appts = await db.Appointments.Where(a => a.OrgId == Org && a.PreferredAt.Date == d.Date
            && a.Status != ApptStatus.Cancelled && a.Status != ApptStatus.NoShow).ToListAsync();

        var items = bays.Select(b =>
        {
            var booked = appts.Count(a => a.BayCode == b.Code);
            return new
            {
                b.Code, b.Name, b.BayType, b.CapacityPerSlot,
                booked, available = Math.Max(0, b.CapacityPerSlot - booked)
            };
        });
        return new { date = d.ToString("yyyy-MM-dd"), bays = bays.Count, items };
    }

    // ===== Loại cuộc hẹn (Mst_Ser_AppType) =====
    public async Task<object> AddAppTypeAsync(AddAppTypeDto dto)
    {
        var code = dto.Code.Trim().ToUpperInvariant();
        var t = await db.AppTypes.FirstOrDefaultAsync(x => x.OrgId == Org && x.Code == code);
        if (t is null) { t = new AppType { OrgId = Org, Code = code, Name = dto.Name.Trim(), Active = true }; db.AppTypes.Add(t); }
        else { t.Name = dto.Name.Trim(); t.Active = true; }
        await db.SaveChangesAsync();
        return new { t.Code, t.Name, t.Active };
    }

    public async Task<object> ListAppTypesAsync(bool? active)
    {
        var q = db.AppTypes.Where(x => x.OrgId == Org);
        if (active.HasValue) q = q.Where(x => x.Active == active.Value);
        var items = await q.OrderBy(x => x.Code).Select(x => new { x.Code, x.Name, x.Active }).ToListAsync();
        return new { count = items.Count, items };
    }

    // ===== Loại khoang sửa chữa (Mst_Compartment / Ser_Cavity.CavityType) =====
    public async Task<object> AddCavityTypeAsync(AddCavityTypeDto dto)
    {
        var code = dto.Code.Trim().ToUpperInvariant();
        var t = await db.CavityTypes.FirstOrDefaultAsync(x => x.OrgId == Org && x.Code == code);
        if (t is null) { t = new CavityType { OrgId = Org, Code = code, Name = dto.Name.Trim(), Active = true }; db.CavityTypes.Add(t); }
        else { t.Name = dto.Name.Trim(); t.Active = true; }
        await db.SaveChangesAsync();
        return new { t.Code, t.Name, t.Active };
    }

    public async Task<object> ListCavityTypesAsync(bool? active)
    {
        var q = db.CavityTypes.Where(x => x.OrgId == Org);
        if (active.HasValue) q = q.Where(x => x.Active == active.Value);
        var items = await q.OrderBy(x => x.Code).Select(x => new { x.Code, x.Name, x.Active }).ToListAsync();
        return new { count = items.Count, items };
    }
}
