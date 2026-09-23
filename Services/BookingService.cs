using Microsoft.EntityFrameworkCore;
using MiniBooking.Data;
using MiniBooking.Models;

namespace MiniBooking.Services;

public record BookDto(string CustomerName, string Phone, string? Vin, string? Plate, string? ServiceType, DateTime PreferredAt, string? DealerCode, string? Note, string? BayCode = null, string? AppTypeCode = null, DateTime? SlotTo = null, string? RoId = null);
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
public record AddServiceItemDto(string SerCode, string? SerName, decimal? StdManHour, string? Note);
public record AddPartItemDto(string PartCode, string? PartName, string? Unit, decimal? Quantity, decimal? InventoryQuantity, string? Note);
public record AddRepairOrderDto(string RoId, string? RoNo, string? DealerCode, string? CusName, string? CusTel, string? PlateNo, string? FrameNo, string? CusRequest, string? Status);
public record SlotQueryDto(string Date, string? BayCode, string? DealerCode);
public record CreatePostCareDto(string CusCareId, string? RoId, string? RoNo, string CustomerName, string? Phone, string? Plate, string? FrameNo, string? DealerCode, DateTime? FinishedDate, string? Note);
public record PostCareContactDto(string? ContactDate, string? FyourCSSH, string? WFBasicNeeds, string? YourCarProblem, string? YourRIWN, string? YourSatisfyQSv, string? YourHopeOfOur, string? Note);
public record CreateReceptionFormDto(string? ReceptionFNo, string? DealerCode, string CustomerName, string? Phone, string? Plate, string? FrameNo, string? ReceptionType, string? AppCode, string? Note);
public record DeliverReceptionFormDto(string? RoNo, string? Note);
// Ser_App_GetStatusList01DL: bộ lọc nâng cao danh sách lịch hẹn (đa giá trị '|', mẫu biển số, khoảng thời gian, timeline).
public record SearchAppointmentsDto(string? DealerCodes, string? Statuses, string? PlatePattern, string? CustomerName, string? Creator, string? AppTypeCodes, string? DateFrom, string? DateTimeline, int? RecordStart, int? RecordCount);
// Ser_App_UpdateDL: sửa lịch hẹn đã có (đổi thời gian/khoang/loại/ghi chú + thay danh sách dịch vụ & phụ tùng).
public record UpdateAppointmentDto(string? CustomerName, string? Phone, string? Vin, string? Plate, string? ServiceType,
    DateTime? PreferredAt, string? DealerCode, string? Note, string? BayCode, string? AppTypeCode, DateTime? SlotTo,
    string? Engineer, List<AddServiceItemDto>? ServiceItems, List<AddPartItemDto>? PartItems);
// Ser_AssignmentWork: phân công công việc sửa chữa cho 1 lệnh sửa chữa (ROID) theo từng công đoạn + KTV.
public record WorkStageDto(string WorkType, string? CavityCode, DateTime? PlanStart, DateTime? PlanFinish, DateTime? ActualStart, DateTime? ActualFinish);
public record WorkEngineerDto(string EngineerCode, string? WorkType);
public record CreateWorkAssignmentDto(string RoId, string? RoNo, string? DealerCode, string? WorkTypeStart, string? WorkTypeFinish,
    List<WorkStageDto>? Stages, List<WorkEngineerDto>? Engineers);
public record UpdateWorkAssignmentDto(string? RoNo, string? DealerCode, string? WorkTypeStart, string? WorkTypeFinish,
    List<WorkStageDto>? Stages, List<WorkEngineerDto>? Engineers);

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
    Task<object?> AddServiceItemAsync(string code, AddServiceItemDto dto);   // gán dịch vụ kèm lịch hẹn (Ser_AppServiceItems)
    Task<object?> ListServiceItemsAsync(string code);                        // danh sách dịch vụ của lịch hẹn
    Task<object?> RemoveServiceItemAsync(string code, long itemId);          // bỏ 1 dịch vụ khỏi lịch hẹn
    Task<object?> AddPartItemAsync(string code, AddPartItemDto dto);         // gán phụ tùng kèm lịch hẹn (Ser_AppPartItems)
    Task<object?> ListPartItemsAsync(string code);                           // danh sách phụ tùng của lịch hẹn
    Task<object?> RemovePartItemAsync(string code, long itemId);             // bỏ 1 phụ tùng khỏi lịch hẹn
    Task<object> AddRepairOrderAsync(AddRepairOrderDto dto);                 // tạo/cập nhật lệnh sửa chữa (Ser_RO)
    Task<object> ListRepairOrdersAsync(string? dealer, bool? linked);        // danh sách lệnh sửa chữa
    Task<object?> GetRepairOrderAsync(string roId);                          // chi tiết 1 lệnh sửa chữa
    Task<object?> LinkRepairOrderAsync(string roId, string appCode);         // gắn lệnh sửa chữa ↔ lịch hẹn (Ser_RO_UpdateAppId)
    Task<object> CreatePostCareAsync(CreatePostCareDto dto);                 // tạo phiếu chăm sóc sau dịch vụ 72h (Ser_CustomerCare72h)
    Task<object> ListPostCaresAsync(string? status, string? dealer, string? dueBefore);  // danh sách phiếu chăm sóc 72h
    Task<object?> GetPostCareAsync(string cusCareId);                        // chi tiết 1 phiếu chăm sóc 72h
    Task<object?> ContactPostCareAsync(string cusCareId, PostCareContactDto dto);  // ghi nhận liên hệ + trả lời khảo sát (CINFB/CIFB)
    Task<object?> RejectPostCareAsync(string cusCareId, string? note);       // bỏ qua không liên hệ (REJ)
    Task<object> PostCareStatsAsync();                                       // thống kê phiếu chăm sóc 72h
    Task<object> CreateReceptionFormAsync(CreateReceptionFormDto dto);       // lập phiếu tiếp nhận xe (Ser_ReceptionF)
    Task<object> ListReceptionFormsAsync(string? status, string? dealer, string? date);  // danh sách phiếu tiếp nhận
    Task<object?> GetReceptionFormAsync(string receptionFNo);                // chi tiết 1 phiếu tiếp nhận
    Task<object?> DeliverReceptionFormAsync(string receptionFNo, DeliverReceptionFormDto dto);  // giao xe (P → A)
    Task<object?> DeleteReceptionFormAsync(string receptionFNo);             // xóa phiếu (chặn khi đã có RO)
    Task<object> SearchAppointmentsAsync(SearchAppointmentsDto dto);         // tìm kiếm nâng cao lịch hẹn (Ser_App_GetStatusList01DL)
    Task<object?> UpdateAppointmentAsync(string code, UpdateAppointmentDto dto);  // sửa lịch hẹn (Ser_App_UpdateDL)
    Task<object> CreateWorkAssignmentAsync(CreateWorkAssignmentDto dto);          // phân công công việc sửa chữa (Ser_AssignmentWork_CreateDL)
    Task<object> ListWorkAssignmentsAsync(string? roId, string? dealer, string? date);  // danh sách phân công (Ser_AssignmentWork_Get_DL)
    Task<object?> GetWorkAssignmentAsync(string roId);                            // chi tiết phân công theo ROID
    Task<object?> UpdateWorkAssignmentAsync(string roId, UpdateWorkAssignmentDto dto);  // sửa phân công (Ser_AssignmentWork_UpdateDL)
    Task<object?> DeleteWorkAssignmentAsync(string roId);                         // xóa phân công (Ser_AssignmentWork_DeleteDL)
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

        // SerAppCreateDL_InvaliddtDateTimeFrom: không cho đặt lịch ở thời điểm đã qua.
        if (dto.PreferredAt < DateTime.Now)
            throw new InvalidOperationException($"Thời gian hẹn {dto.PreferredAt:yyyy-MM-dd HH:mm} đã qua, vui lòng chọn thời gian khác.");
        // SerAppCreateDL_InvaliddtDateTimeTo: giờ kết thúc (nếu có) phải sau giờ bắt đầu.
        if (dto.SlotTo.HasValue && dto.SlotTo.Value <= dto.PreferredAt)
            throw new InvalidOperationException($"Giờ kết thúc {dto.SlotTo:HH:mm} phải sau giờ bắt đầu {dto.PreferredAt:HH:mm}.");

        // SerAppCreateDL_AppTypeCodeNotEmpty: loại cuộc hẹn phải có và tồn tại trong master Mst_Ser_AppType.
        var appTypeCode = dto.AppTypeCode?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(appTypeCode))
        {
            var ok = await db.AppTypes.AnyAsync(x => x.OrgId == Org && x.Code == appTypeCode && x.Active);
            if (!ok) throw new InvalidOperationException($"Loại cuộc hẹn '{appTypeCode}' không tồn tại hoặc đã ngừng dùng.");
        }

        // SerAppCreateDL_InvalidROID / SerAppCreateDL_ROExistAppId: nếu đặt lịch từ 1 lệnh sửa chữa (ROID),
        // RO phải tồn tại và chưa gắn cuộc hẹn nào (Ser_RO.AppId rỗng).
        var roId = dto.RoId?.Trim().ToUpperInvariant();
        RepairOrder? ro = null;
        if (!string.IsNullOrWhiteSpace(roId))
        {
            ro = await db.RepairOrders.FirstOrDefaultAsync(x => x.OrgId == Org && x.RoId == roId);
            if (ro is null) throw new InvalidOperationException($"Lệnh sửa chữa '{roId}' không tồn tại.");
            if (!string.IsNullOrWhiteSpace(ro.AppCode))
                throw new InvalidOperationException($"Lệnh sửa chữa '{roId}' đã gắn cuộc hẹn {ro.AppCode}.");
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
            BayCode = bayCode, AppTypeCode = appTypeCode, RoId = roId,
            Status = ApptStatus.Requested
        };
        db.Appointments.Add(a);
        // Ser_RO_UpdateAppId: gắn ngược AppId vào lệnh sửa chữa nguồn.
        if (ro is not null) { ro.AppCode = a.Code; ro.LinkedAt = DateTime.Now; }
        await db.SaveChangesAsync();
        return new { a.Code, status = a.Status.ToString(), statusText = Text(a.Status), a.PreferredAt, a.BayCode, a.AppTypeCode, a.RoId };
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

    // ===== Dịch vụ đăng ký kèm lịch hẹn (Ser_AppServiceItems) =====
    // Gán 1 công việc (SerCode/SerName/StdManHour) vào lịch hẹn; dedupe theo SerCode.
    public async Task<object?> AddServiceItemAsync(string code, AddServiceItemDto dto)
    {
        var a = await Get(code);
        if (a is null) return null;
        if (string.IsNullOrWhiteSpace(dto.SerCode)) throw new InvalidOperationException("Cần SerCode (mã công việc).");
        var serCode = dto.SerCode.Trim().ToUpperInvariant();
        var item = await db.AppServiceItems.FirstOrDefaultAsync(x => x.OrgId == Org && x.AppCode == a.Code && x.SerCode == serCode);
        if (item is null)
        {
            item = new AppServiceItem
            {
                OrgId = Org, AppCode = a.Code, SerCode = serCode,
                SerName = string.IsNullOrWhiteSpace(dto.SerName) ? serCode : dto.SerName!.Trim(),
                StdManHour = dto.StdManHour is > 0 ? dto.StdManHour!.Value : 0m,
                Note = dto.Note
            };
            db.AppServiceItems.Add(item);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(dto.SerName)) item.SerName = dto.SerName!.Trim();
            if (dto.StdManHour is > 0) item.StdManHour = dto.StdManHour!.Value;
            if (dto.Note != null) item.Note = dto.Note;
        }
        await db.SaveChangesAsync();
        return new { item.Id, item.AppCode, item.SerCode, item.SerName, item.StdManHour, item.Note };
    }

    // Danh sách dịch vụ của 1 lịch hẹn + tổng giờ công chuẩn (ước lượng thời lượng).
    public async Task<object?> ListServiceItemsAsync(string code)
    {
        var a = await Get(code);
        if (a is null) return null;
        var items = await db.AppServiceItems.Where(x => x.OrgId == Org && x.AppCode == a.Code)
            .OrderBy(x => x.SerCode)
            .Select(x => new { x.Id, x.SerCode, x.SerName, x.StdManHour, x.Note }).ToListAsync();
        return new { a.Code, count = items.Count, totalManHour = items.Sum(x => x.StdManHour), items };
    }

    public async Task<object?> RemoveServiceItemAsync(string code, long itemId)
    {
        var a = await Get(code);
        if (a is null) return null;
        var item = await db.AppServiceItems.FirstOrDefaultAsync(x => x.OrgId == Org && x.AppCode == a.Code && x.Id == itemId);
        if (item is null) return null;
        db.AppServiceItems.Remove(item);
        await db.SaveChangesAsync();
        return new { a.Code, removed = itemId };
    }

    // ===== Phụ tùng đăng ký kèm lịch hẹn (Ser_AppPartItems) =====
    // Gán 1 phụ tùng (PartCode/PartName/Unit/Quantity) vào lịch hẹn; dedupe theo PartCode.
    public async Task<object?> AddPartItemAsync(string code, AddPartItemDto dto)
    {
        var a = await Get(code);
        if (a is null) return null;
        if (string.IsNullOrWhiteSpace(dto.PartCode)) throw new InvalidOperationException("Cần PartCode (mã phụ tùng).");
        var partCode = dto.PartCode.Trim().ToUpperInvariant();
        var item = await db.AppPartItems.FirstOrDefaultAsync(x => x.OrgId == Org && x.AppCode == a.Code && x.PartCode == partCode);
        if (item is null)
        {
            item = new AppPartItem
            {
                OrgId = Org, AppCode = a.Code, PartCode = partCode,
                PartName = string.IsNullOrWhiteSpace(dto.PartName) ? partCode : dto.PartName!.Trim(),
                Unit = dto.Unit?.Trim() ?? "",
                Quantity = dto.Quantity is > 0 ? dto.Quantity!.Value : 0m,
                InventoryQuantity = dto.InventoryQuantity is > 0 ? dto.InventoryQuantity!.Value : 0m,
                Note = dto.Note
            };
            db.AppPartItems.Add(item);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(dto.PartName)) item.PartName = dto.PartName!.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Unit)) item.Unit = dto.Unit!.Trim();
            if (dto.Quantity is > 0) item.Quantity = dto.Quantity!.Value;
            if (dto.InventoryQuantity is > 0) item.InventoryQuantity = dto.InventoryQuantity!.Value;
            if (dto.Note != null) item.Note = dto.Note;
        }
        await db.SaveChangesAsync();
        return new { item.Id, item.AppCode, item.PartCode, item.PartName, item.Unit, item.Quantity, item.InventoryQuantity, item.Note };
    }

    // Danh sách phụ tùng của 1 lịch hẹn + cờ thiếu tồn (Quantity > InventoryQuantity) để chuẩn bị trước.
    public async Task<object?> ListPartItemsAsync(string code)
    {
        var a = await Get(code);
        if (a is null) return null;
        var items = await db.AppPartItems.Where(x => x.OrgId == Org && x.AppCode == a.Code)
            .OrderBy(x => x.PartCode)
            .Select(x => new { x.Id, x.PartCode, x.PartName, x.Unit, x.Quantity, x.InventoryQuantity, x.Note }).ToListAsync();
        var rows = items.Select(x => new
        {
            x.Id, x.PartCode, x.PartName, x.Unit, x.Quantity, x.InventoryQuantity, x.Note,
            shortage = x.Quantity > x.InventoryQuantity
        });
        return new { a.Code, count = items.Count, totalQuantity = items.Sum(x => x.Quantity), items = rows };
    }

    public async Task<object?> RemovePartItemAsync(string code, long itemId)
    {
        var a = await Get(code);
        if (a is null) return null;
        var item = await db.AppPartItems.FirstOrDefaultAsync(x => x.OrgId == Org && x.AppCode == a.Code && x.Id == itemId);
        if (item is null) return null;
        db.AppPartItems.Remove(item);
        await db.SaveChangesAsync();
        return new { a.Code, removed = itemId };
    }

    // ===== Lệnh sửa chữa / báo giá (Ser_RO) =====
    // Tạo/cập nhật 1 lệnh sửa chữa; dedupe theo RoId. Dùng làm nguồn để đặt lịch hẹn (Ser_App.ROID).
    public async Task<object> AddRepairOrderAsync(AddRepairOrderDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RoId)) throw new InvalidOperationException("Cần RoId (mã lệnh sửa chữa).");
        var roId = dto.RoId.Trim().ToUpperInvariant();
        var ro = await db.RepairOrders.FirstOrDefaultAsync(x => x.OrgId == Org && x.RoId == roId);
        if (ro is null)
        {
            ro = new RepairOrder
            {
                OrgId = Org, RoId = roId,
                RoNo = string.IsNullOrWhiteSpace(dto.RoNo) ? roId : dto.RoNo!.Trim(),
                DealerCode = dto.DealerCode?.Trim() ?? "",
                CusName = dto.CusName?.Trim() ?? "",
                CusTel = dto.CusTel?.Trim(), PlateNo = dto.PlateNo?.Trim(), FrameNo = dto.FrameNo?.Trim(),
                CusRequest = dto.CusRequest, Status = string.IsNullOrWhiteSpace(dto.Status) ? "Open" : dto.Status!.Trim()
            };
            db.RepairOrders.Add(ro);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(dto.RoNo)) ro.RoNo = dto.RoNo!.Trim();
            if (!string.IsNullOrWhiteSpace(dto.DealerCode)) ro.DealerCode = dto.DealerCode!.Trim();
            if (!string.IsNullOrWhiteSpace(dto.CusName)) ro.CusName = dto.CusName!.Trim();
            if (dto.CusTel != null) ro.CusTel = dto.CusTel.Trim();
            if (dto.PlateNo != null) ro.PlateNo = dto.PlateNo.Trim();
            if (dto.FrameNo != null) ro.FrameNo = dto.FrameNo.Trim();
            if (dto.CusRequest != null) ro.CusRequest = dto.CusRequest;
            if (!string.IsNullOrWhiteSpace(dto.Status)) ro.Status = dto.Status!.Trim();
        }
        await db.SaveChangesAsync();
        return new { ro.RoId, ro.RoNo, ro.DealerCode, ro.CusName, ro.PlateNo, ro.Status, ro.AppCode, ro.LinkedAt };
    }

    // Danh sách lệnh sửa chữa; linked=true → đã gắn cuộc hẹn, false → chưa gắn.
    public async Task<object> ListRepairOrdersAsync(string? dealer, bool? linked)
    {
        var q = db.RepairOrders.Where(x => x.OrgId == Org);
        if (!string.IsNullOrWhiteSpace(dealer)) q = q.Where(x => x.DealerCode == dealer);
        if (linked.HasValue) q = linked.Value ? q.Where(x => x.AppCode != null) : q.Where(x => x.AppCode == null);
        var items = await q.OrderBy(x => x.RoId).Take(500).Select(x => new
        {
            x.RoId, x.RoNo, x.DealerCode, x.CusName, x.CusTel, x.PlateNo, x.FrameNo, x.Status, x.AppCode, x.LinkedAt, x.CreatedAt
        }).ToListAsync();
        return new { count = items.Count, items };
    }

    public async Task<object?> GetRepairOrderAsync(string roId)
    {
        roId = roId.Trim().ToUpperInvariant();
        var ro = await db.RepairOrders.FirstOrDefaultAsync(x => x.OrgId == Org && x.RoId == roId);
        if (ro is null) return null;
        return new { ro.RoId, ro.RoNo, ro.DealerCode, ro.CusName, ro.CusTel, ro.PlateNo, ro.FrameNo, ro.CusRequest, ro.Status, ro.AppCode, ro.LinkedAt, ro.CreatedAt };
    }

    // Ser_RO_UpdateAppId: gắn 1 lệnh sửa chữa với 1 lịch hẹn (đặt AppId cho RO và ROID cho lịch hẹn).
    public async Task<object?> LinkRepairOrderAsync(string roId, string appCode)
    {
        roId = roId.Trim().ToUpperInvariant();
        appCode = appCode.Trim().ToUpperInvariant();
        var ro = await db.RepairOrders.FirstOrDefaultAsync(x => x.OrgId == Org && x.RoId == roId);
        if (ro is null) return null;
        var a = await db.Appointments.FirstOrDefaultAsync(x => x.OrgId == Org && x.Code == appCode);
        if (a is null) return null;
        if (!string.IsNullOrWhiteSpace(ro.AppCode) && ro.AppCode != appCode)
            throw new InvalidOperationException($"Lệnh sửa chữa '{roId}' đã gắn cuộc hẹn {ro.AppCode}.");
        ro.AppCode = a.Code; ro.LinkedAt = DateTime.Now;
        a.RoId = ro.RoId;
        await db.SaveChangesAsync();
        return new { ro.RoId, ro.AppCode, appCode = a.Code, appRoId = a.RoId, ro.LinkedAt };
    }

    // ===== Chăm sóc KH sau dịch vụ 72h (Ser_CustomerCare72h) =====
    // Tạo phiếu khảo sát hài lòng sau khi giao xe; dedupe theo CusCareId.
    public async Task<object> CreatePostCareAsync(CreatePostCareDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CusCareId)) throw new InvalidOperationException("Cần CusCareId (mã phiếu chăm sóc).");
        var cusCareId = dto.CusCareId.Trim().ToUpperInvariant();
        var c = await db.PostServiceCares.FirstOrDefaultAsync(x => x.OrgId == Org && x.CusCareId == cusCareId);
        if (c is null)
        {
            c = new PostServiceCare
            {
                OrgId = Org, CusCareId = cusCareId,
                RoId = dto.RoId?.Trim().ToUpperInvariant(), RoNo = dto.RoNo?.Trim(),
                CustomerName = dto.CustomerName?.Trim() ?? "", Phone = dto.Phone?.Trim(),
                Plate = dto.Plate?.Trim(), FrameNo = dto.FrameNo?.Trim(),
                DealerCode = dto.DealerCode?.Trim() ?? "", FinishedDate = dto.FinishedDate,
                Note = dto.Note, Status = "PEND"
            };
            db.PostServiceCares.Add(c);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(dto.CustomerName)) c.CustomerName = dto.CustomerName!.Trim();
            if (dto.Phone != null) c.Phone = dto.Phone.Trim();
            if (dto.Plate != null) c.Plate = dto.Plate.Trim();
            if (dto.FrameNo != null) c.FrameNo = dto.FrameNo.Trim();
            if (!string.IsNullOrWhiteSpace(dto.DealerCode)) c.DealerCode = dto.DealerCode!.Trim();
            if (dto.RoId != null) c.RoId = dto.RoId.Trim().ToUpperInvariant();
            if (dto.RoNo != null) c.RoNo = dto.RoNo.Trim();
            if (dto.FinishedDate.HasValue) c.FinishedDate = dto.FinishedDate;
            if (dto.Note != null) c.Note = dto.Note;
        }
        await db.SaveChangesAsync();
        return new { c.CusCareId, c.RoId, c.CustomerName, c.Status, c.FinishedDate };
    }

    // Danh sách phiếu chăm sóc 72h; dueBefore lọc theo mốc giao xe (FinishedDate) đến hạn.
    public async Task<object> ListPostCaresAsync(string? status, string? dealer, string? dueBefore)
    {
        var q = db.PostServiceCares.Where(x => x.OrgId == Org);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status.ToUpperInvariant());
        if (!string.IsNullOrWhiteSpace(dealer)) q = q.Where(x => x.DealerCode == dealer);
        if (!string.IsNullOrWhiteSpace(dueBefore) && DateTime.TryParse(dueBefore, out var d)) q = q.Where(x => x.FinishedDate != null && x.FinishedDate.Value.Date <= d.Date);
        var items = await q.OrderBy(x => x.FinishedDate).Take(500).Select(x => new
        {
            x.CusCareId, x.RoId, x.RoNo, x.CustomerName, x.Phone, x.Plate, x.FrameNo, x.DealerCode,
            x.FinishedDate, x.Status, x.ContactDate, x.YourSatisfyQSv, x.YourRIWN, x.Note
        }).ToListAsync();
        return new { count = items.Count, items };
    }

    public async Task<object?> GetPostCareAsync(string cusCareId)
    {
        cusCareId = cusCareId.Trim().ToUpperInvariant();
        var c = await db.PostServiceCares.FirstOrDefaultAsync(x => x.OrgId == Org && x.CusCareId == cusCareId);
        if (c is null) return null;
        return new { c.CusCareId, c.RoId, c.RoNo, c.CustomerName, c.Phone, c.Plate, c.FrameNo, c.DealerCode, c.FinishedDate, c.Status, c.ContactDate, c.FyourCSSH, c.WFBasicNeeds, c.YourCarProblem, c.YourRIWN, c.YourSatisfyQSv, c.YourHopeOfOur, c.Note, c.CreatedAt };
    }

    // Ghi nhận liên hệ + trả lời khảo sát: PEND → CIFB (đã phản hồi) nếu có câu trả lời, ngược lại CINFB (chưa phản hồi).
    public async Task<object?> ContactPostCareAsync(string cusCareId, PostCareContactDto dto)
    {
        cusCareId = cusCareId.Trim().ToUpperInvariant();
        var c = await db.PostServiceCares.FirstOrDefaultAsync(x => x.OrgId == Org && x.CusCareId == cusCareId);
        if (c is null || c.Status == "REJ") return null;
        if (!string.IsNullOrWhiteSpace(dto.FyourCSSH)) c.FyourCSSH = dto.FyourCSSH!.Trim();
        if (!string.IsNullOrWhiteSpace(dto.WFBasicNeeds)) c.WFBasicNeeds = dto.WFBasicNeeds!.Trim();
        if (!string.IsNullOrWhiteSpace(dto.YourCarProblem)) c.YourCarProblem = dto.YourCarProblem!.Trim();
        if (!string.IsNullOrWhiteSpace(dto.YourRIWN)) c.YourRIWN = dto.YourRIWN!.Trim();
        if (!string.IsNullOrWhiteSpace(dto.YourSatisfyQSv)) c.YourSatisfyQSv = dto.YourSatisfyQSv!.Trim();
        if (!string.IsNullOrWhiteSpace(dto.YourHopeOfOur)) c.YourHopeOfOur = dto.YourHopeOfOur!.Trim();
        if (dto.Note != null) c.Note = dto.Note;
        c.ContactDate = string.IsNullOrWhiteSpace(dto.ContactDate) ? DateTime.Now : (DateTime.TryParse(dto.ContactDate, out var cd) ? cd : DateTime.Now);
        var answered = !string.IsNullOrWhiteSpace(c.YourSatisfyQSv) || !string.IsNullOrWhiteSpace(c.YourCarProblem)
            || !string.IsNullOrWhiteSpace(c.YourRIWN) || !string.IsNullOrWhiteSpace(c.FyourCSSH)
            || !string.IsNullOrWhiteSpace(c.WFBasicNeeds) || !string.IsNullOrWhiteSpace(c.YourHopeOfOur);
        c.Status = answered ? "CIFB" : "CINFB";
        await db.SaveChangesAsync();
        return new { c.CusCareId, c.Status, c.ContactDate, c.YourSatisfyQSv, c.YourRIWN };
    }

    // Bỏ qua không cần liên hệ (REJ).
    public async Task<object?> RejectPostCareAsync(string cusCareId, string? note)
    {
        cusCareId = cusCareId.Trim().ToUpperInvariant();
        var c = await db.PostServiceCares.FirstOrDefaultAsync(x => x.OrgId == Org && x.CusCareId == cusCareId);
        if (c is null) return null;
        c.Status = "REJ";
        if (!string.IsNullOrWhiteSpace(note)) c.Note = note;
        await db.SaveChangesAsync();
        return new { c.CusCareId, c.Status, c.Note };
    }

    public async Task<object> PostCareStatsAsync()
    {
        var q = db.PostServiceCares.Where(x => x.OrgId == Org);
        var today = DateTime.Now.Date;
        return new
        {
            total = await q.CountAsync(),
            pending = await q.CountAsync(x => x.Status == "PEND"),
            contactedNoFeedback = await q.CountAsync(x => x.Status == "CINFB"),
            contactedFeedback = await q.CountAsync(x => x.Status == "CIFB"),
            rejected = await q.CountAsync(x => x.Status == "REJ"),
            overdue = await q.CountAsync(x => x.Status == "PEND" && x.FinishedDate != null && x.FinishedDate.Value.Date < today)
        };
    }

    // ===== Phiếu tiếp nhận xe (Ser_ReceptionF) =====
    // Lập phiếu khi khách đến xưởng; dedupe theo ReceptionFNo (tự sinh nếu bỏ trống).
    public async Task<object> CreateReceptionFormAsync(CreateReceptionFormDto dto)
    {
        var no = string.IsNullOrWhiteSpace(dto.ReceptionFNo)
            ? "RF" + DateTime.Now.ToString("yyMMddHHmmss") + Random.Shared.Next(10, 99)
            : dto.ReceptionFNo!.Trim().ToUpperInvariant();
        var rf = await db.ReceptionForms.FirstOrDefaultAsync(x => x.OrgId == Org && x.ReceptionFNo == no);
        if (rf is null)
        {
            rf = new ReceptionForm
            {
                OrgId = Org, ReceptionFNo = no,
                DealerCode = dto.DealerCode?.Trim() ?? "",
                CusName = dto.CustomerName?.Trim() ?? "",
                Phone = dto.Phone?.Trim(), Plate = dto.Plate?.Trim(), FrameNo = dto.FrameNo?.Trim(),
                ReceptionType = string.IsNullOrWhiteSpace(dto.ReceptionType) ? "Service" : dto.ReceptionType!.Trim(),
                Status = "P", CreatedDateTime = DateTime.Now,
                AppCode = dto.AppCode?.Trim().ToUpperInvariant(), Note = dto.Note
            };
            db.ReceptionForms.Add(rf);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(dto.CustomerName)) rf.CusName = dto.CustomerName!.Trim();
            if (dto.Phone != null) rf.Phone = dto.Phone.Trim();
            if (dto.Plate != null) rf.Plate = dto.Plate.Trim();
            if (dto.FrameNo != null) rf.FrameNo = dto.FrameNo.Trim();
            if (!string.IsNullOrWhiteSpace(dto.ReceptionType)) rf.ReceptionType = dto.ReceptionType!.Trim();
            if (!string.IsNullOrWhiteSpace(dto.DealerCode)) rf.DealerCode = dto.DealerCode!.Trim();
            if (dto.AppCode != null) rf.AppCode = dto.AppCode.Trim().ToUpperInvariant();
            if (dto.Note != null) rf.Note = dto.Note;
        }
        await db.SaveChangesAsync();
        return new { rf.ReceptionFNo, rf.CusName, rf.Plate, rf.ReceptionType, rf.Status, rf.AppCode, rf.CreatedDateTime };
    }

    // Danh sách phiếu tiếp nhận; lọc theo trạng thái (P/A), xưởng, ngày lập.
    public async Task<object> ListReceptionFormsAsync(string? status, string? dealer, string? date)
    {
        var q = db.ReceptionForms.Where(x => x.OrgId == Org);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status.Trim().ToUpperInvariant());
        if (!string.IsNullOrWhiteSpace(dealer)) q = q.Where(x => x.DealerCode == dealer);
        if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var d)) q = q.Where(x => x.CreatedDateTime.Date == d.Date);
        var items = await q.OrderByDescending(x => x.CreatedDateTime).Take(500).Select(x => new
        {
            x.ReceptionFNo, x.DealerCode, x.CusName, x.Phone, x.Plate, x.FrameNo, x.ReceptionType,
            x.Status, statusText = x.Status == "A" ? "Giao xe" : "Tiếp nhận",
            x.CreatedBy, x.CreatedDateTime, x.DeliveryDateTime, x.AppCode, x.RoNo, x.Note
        }).ToListAsync();
        return new { count = items.Count, items };
    }

    public async Task<object?> GetReceptionFormAsync(string receptionFNo)
    {
        receptionFNo = receptionFNo.Trim().ToUpperInvariant();
        var rf = await db.ReceptionForms.FirstOrDefaultAsync(x => x.OrgId == Org && x.ReceptionFNo == receptionFNo);
        if (rf is null) return null;
        return new { rf.ReceptionFNo, rf.DealerCode, rf.CusName, rf.Phone, rf.Plate, rf.FrameNo, rf.ReceptionType, rf.Status, rf.CreatedBy, rf.CreatedDateTime, rf.DeliveryDateTime, rf.AppCode, rf.RoNo, rf.Note };
    }

    // Giao xe: P (Tiếp nhận) → A (Giao xe), ghi thời điểm giao + số RO (nếu có).
    public async Task<object?> DeliverReceptionFormAsync(string receptionFNo, DeliverReceptionFormDto dto)
    {
        receptionFNo = receptionFNo.Trim().ToUpperInvariant();
        var rf = await db.ReceptionForms.FirstOrDefaultAsync(x => x.OrgId == Org && x.ReceptionFNo == receptionFNo);
        if (rf is null || rf.Status == "A") return null;
        rf.Status = "A";
        rf.DeliveryDateTime = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(dto.RoNo)) rf.RoNo = dto.RoNo!.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Note)) rf.Note = dto.Note;
        await db.SaveChangesAsync();
        return new { rf.ReceptionFNo, rf.Status, rf.DeliveryDateTime, rf.RoNo };
    }

    // Ser_ReceptionF_DeleteX_ExistRONotDelete: chỉ xóa được khi phiếu chưa phát sinh lệnh sửa chữa (RO).
    public async Task<object?> DeleteReceptionFormAsync(string receptionFNo)
    {
        receptionFNo = receptionFNo.Trim().ToUpperInvariant();
        var rf = await db.ReceptionForms.FirstOrDefaultAsync(x => x.OrgId == Org && x.ReceptionFNo == receptionFNo);
        if (rf is null) return null;
        // Đã gắn RO (qua RoNo trên phiếu hoặc RO tham chiếu AppCode) → không cho xóa.
        var hasRo = !string.IsNullOrWhiteSpace(rf.RoNo)
            || (rf.AppCode != null && await db.RepairOrders.AnyAsync(x => x.OrgId == Org && x.AppCode == rf.AppCode));
        if (hasRo)
            throw new InvalidOperationException($"Phiếu tiếp nhận '{receptionFNo}' đã phát sinh lệnh sửa chữa, không thể xóa.");
        db.ReceptionForms.Remove(rf);
        await db.SaveChangesAsync();
        return new { rf.ReceptionFNo, deleted = true };
    }

    // ===== Tìm kiếm nâng cao lịch hẹn (Ser_App_GetStatusList01DL) =====
    // Bộ lọc đa giá trị (phân tách '|'), mẫu biển số (LIKE), tên KH (chứa), người tạo, loại cuộc hẹn,
    // thời gian từ (AppDateTimeFrom >=), timeline (lịch bao trùm 1 mốc: From <= T <= To) + phân trang.
    public async Task<object> SearchAppointmentsAsync(SearchAppointmentsDto dto)
    {
        var q = db.Appointments.Where(a => a.OrgId == Org);

        // DealerCodeList: '|'-separated → IN (...).
        var dealers = SplitList(dto.DealerCodes);
        if (dealers.Length > 0) q = q.Where(a => dealers.Contains(a.DealerCode));

        // AppStatusList: '|'-separated mã trạng thái nguồn (1..5) → map sang ApptStatus.
        var statuses = SplitList(dto.Statuses).Select(ParseSourceStatus).Where(s => s.HasValue).Select(s => s!.Value).ToArray();
        if (statuses.Length > 0) q = q.Where(a => statuses.Contains(a.Status));

        // PlateNoPattern: LIKE (chứa) trên biển số.
        if (!string.IsNullOrWhiteSpace(dto.PlatePattern))
        {
            var p = dto.PlatePattern.Trim();
            q = q.Where(a => a.Plate != null && a.Plate.Contains(p));
        }

        // CusName: chứa (không phân biệt hoa thường theo hành vi DB).
        if (!string.IsNullOrWhiteSpace(dto.CustomerName))
        {
            var n = dto.CustomerName.Trim();
            q = q.Where(a => a.CustomerName.Contains(n));
        }

        // Creator: người tạo lịch (MiniBooking chưa lưu Creator → lọc theo Engineer như proxy nếu có).
        if (!string.IsNullOrWhiteSpace(dto.Creator))
        {
            var c = dto.Creator.Trim();
            q = q.Where(a => a.Engineer == c);
        }

        // AppTypeCodeList: '|'-separated → IN (...).
        var appTypes = SplitList(dto.AppTypeCodes);
        if (appTypes.Length > 0) q = q.Where(a => a.AppTypeCode != null && appTypes.Contains(a.AppTypeCode));

        // AppDateTimeFrom: lịch có giờ hẹn từ mốc này trở đi.
        if (!string.IsNullOrWhiteSpace(dto.DateFrom) && DateTime.TryParse(dto.DateFrom, out var from))
            q = q.Where(a => a.PreferredAt >= from);

        // DateTimeline: lịch bao trùm mốc T (SlotFrom <= T <= SlotTo, fallback PreferredAt).
        if (!string.IsNullOrWhiteSpace(dto.DateTimeline) && DateTime.TryParse(dto.DateTimeline, out var tl))
            q = q.Where(a => (a.SlotFrom ?? a.PreferredAt) <= tl && (a.SlotTo ?? a.PreferredAt.AddHours(1)) >= tl);

        var total = await q.CountAsync();

        // Phân trang (RecordStart 0-based, RecordCount mặc định 50, tối đa 500).
        var start = Math.Max(0, dto.RecordStart ?? 0);
        var count = Math.Clamp(dto.RecordCount ?? 50, 1, 500);
        var items = await q.OrderBy(a => a.PreferredAt).ThenBy(a => a.Id)
            .Skip(start).Take(count)
            .Select(a => new
            {
                a.Code, a.CustomerName, a.Phone, a.Vin, a.Plate, a.ServiceType, a.PreferredAt,
                a.DealerCode, a.Engineer, status = a.Status.ToString(), statusText = Text(a.Status),
                a.RoNo, a.BayCode, a.AppTypeCode, a.SlotFrom, a.SlotTo, a.ContactedAt, a.ContactResult
            }).ToListAsync();

        return new { total, recordStart = start, recordCount = count, count = items.Count, items };
    }

    // Tách danh sách '|'-separated thành mảng đã trim/upper, bỏ rỗng (Ser_App_GetStatusList01DL).
    private static string[] SplitList(string? raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? Array.Empty<string>()
            : raw.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                 .Select(x => x.ToUpperInvariant()).ToArray();

    // Map mã trạng thái nguồn (Ser_App.AppStatus: 1 Mới tạo, 2 Xác nhận, 3 Tiếp nhận, 4 Hủy, 5 Đã liên hệ) → ApptStatus.
    private static ApptStatus? ParseSourceStatus(string code) => code switch
    {
        "1" => ApptStatus.Requested,
        "2" => ApptStatus.Confirmed,
        "3" => ApptStatus.CheckedIn,
        "4" => ApptStatus.Cancelled,
        "5" => ApptStatus.Contacted,
        _ => null
    };

    // ===== Sửa lịch hẹn (Ser_App_UpdateDL / Ser_App_UpdateX) =====
    // Cập nhật thông tin lịch hẹn đã có: khách/xe/dịch vụ/thời gian/khoang/loại/ghi chú.
    // Nếu truyền ServiceItems/PartItems thì THAY toàn bộ danh sách cũ (giống Ser_App_UpdateX: delete olds → insert).
    // Áp lại các ràng buộc như khi tạo: thời gian chưa qua, giờ kết thúc > giờ bắt đầu, khoang tồn tại + phù hợp
    // loại dịch vụ + không trùng khung giờ (MyCheck_DateTime_Cavity), loại cuộc hẹn phải có trong master.
    public async Task<object?> UpdateAppointmentAsync(string code, UpdateAppointmentDto dto)
    {
        var a = await Get(code);
        if (a is null) return null;
        // Không sửa lịch đã hoàn tất/đã hủy/không đến.
        if (a.Status is ApptStatus.Done or ApptStatus.Cancelled or ApptStatus.NoShow) return null;

        var serviceType = string.IsNullOrWhiteSpace(dto.ServiceType) ? a.ServiceType : dto.ServiceType!.Trim();
        var preferredAt = dto.PreferredAt ?? a.PreferredAt;

        // SerAppCreateDL_InvaliddtDateTimeFrom: không cho dời lịch về thời điểm đã qua.
        if (preferredAt < DateTime.Now)
            throw new InvalidOperationException($"Thời gian hẹn {preferredAt:yyyy-MM-dd HH:mm} đã qua, vui lòng chọn thời gian khác.");
        // SerAppCreateDL_InvaliddtDateTimeTo: giờ kết thúc (nếu có) phải sau giờ bắt đầu.
        var slotTo = dto.SlotTo ?? a.SlotTo;
        if (slotTo.HasValue && slotTo.Value <= preferredAt)
            throw new InvalidOperationException($"Giờ kết thúc {slotTo:HH:mm} phải sau giờ bắt đầu {preferredAt:HH:mm}.");

        // SerAppCreateDL_AppTypeCodeNotEmpty: loại cuộc hẹn phải tồn tại trong master Mst_Ser_AppType.
        var appTypeCode = dto.AppTypeCode is null ? a.AppTypeCode : dto.AppTypeCode.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(appTypeCode))
        {
            var ok = await db.AppTypes.AnyAsync(x => x.OrgId == Org && x.Code == appTypeCode && x.Active);
            if (!ok) throw new InvalidOperationException($"Loại cuộc hẹn '{appTypeCode}' không tồn tại hoặc đã ngừng dùng.");
        }

        // Khoang: kiểm tra tồn tại + phù hợp loại dịch vụ + không trùng khung giờ (MyCheck_DateTime_Cavity).
        var bayCode = dto.BayCode is null ? a.BayCode : dto.BayCode.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(bayCode))
        {
            var bay = await db.ServiceBays.FirstOrDefaultAsync(x => x.OrgId == Org && x.Code == bayCode && x.Active);
            if (bay is null) throw new InvalidOperationException($"Khoang '{bayCode}' không tồn tại hoặc đã ngừng dùng.");
            if (!CavityRules.IsCompatible(serviceType, bay.BayType))
                throw new InvalidOperationException($"Khoang '{bayCode}' (loại {bay.BayType}) không phù hợp với dịch vụ '{serviceType}'.");
            var from = a.SlotFrom ?? preferredAt;
            var to = slotTo ?? from.AddHours(1);
            var conflict = await FindBayConflictAsync(bayCode, from, to, a.Id);
            if (conflict is not null)
                throw new InvalidOperationException($"Khoang '{bayCode}' đã có lịch {conflict} trùng khung giờ {from:HH:mm}-{to:HH:mm}.");
        }

        // Cập nhật các trường thông tin (chỉ ghi đè khi client truyền giá trị).
        if (!string.IsNullOrWhiteSpace(dto.CustomerName)) a.CustomerName = dto.CustomerName!.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Phone)) a.Phone = dto.Phone!.Trim();
        if (dto.Vin != null) a.Vin = dto.Vin.Trim().ToUpperInvariant();
        if (dto.Plate != null) a.Plate = dto.Plate.Trim();
        a.ServiceType = serviceType;
        a.PreferredAt = preferredAt;
        if (!string.IsNullOrWhiteSpace(dto.DealerCode)) a.DealerCode = dto.DealerCode!.Trim();
        if (dto.Note != null) a.Note = dto.Note;
        a.BayCode = bayCode;
        a.AppTypeCode = appTypeCode;
        if (dto.SlotTo != null) a.SlotTo = dto.SlotTo;
        if (dto.Engineer != null) a.Engineer = dto.Engineer;

        // Ser_App_UpdateX: thay toàn bộ danh sách dịch vụ kèm lịch (delete olds → insert).
        if (dto.ServiceItems is not null)
        {
            var olds = await db.AppServiceItems.Where(x => x.OrgId == Org && x.AppCode == a.Code).ToListAsync();
            db.AppServiceItems.RemoveRange(olds);
            foreach (var it in dto.ServiceItems)
            {
                if (string.IsNullOrWhiteSpace(it.SerCode)) continue;
                var serCode = it.SerCode.Trim().ToUpperInvariant();
                db.AppServiceItems.Add(new AppServiceItem
                {
                    OrgId = Org, AppCode = a.Code, SerCode = serCode,
                    SerName = string.IsNullOrWhiteSpace(it.SerName) ? serCode : it.SerName!.Trim(),
                    StdManHour = it.StdManHour is > 0 ? it.StdManHour!.Value : 0m,
                    Note = it.Note
                });
            }
        }

        // Ser_App_UpdateX: thay toàn bộ danh sách phụ tùng kèm lịch (delete olds → insert).
        if (dto.PartItems is not null)
        {
            var olds = await db.AppPartItems.Where(x => x.OrgId == Org && x.AppCode == a.Code).ToListAsync();
            db.AppPartItems.RemoveRange(olds);
            foreach (var it in dto.PartItems)
            {
                if (string.IsNullOrWhiteSpace(it.PartCode)) continue;
                var partCode = it.PartCode.Trim().ToUpperInvariant();
                db.AppPartItems.Add(new AppPartItem
                {
                    OrgId = Org, AppCode = a.Code, PartCode = partCode,
                    PartName = string.IsNullOrWhiteSpace(it.PartName) ? partCode : it.PartName!.Trim(),
                    Unit = it.Unit?.Trim() ?? "",
                    Quantity = it.Quantity is > 0 ? it.Quantity!.Value : 0m,
                    InventoryQuantity = it.InventoryQuantity is > 0 ? it.InventoryQuantity!.Value : 0m,
                    Note = it.Note
                });
            }
        }

        await db.SaveChangesAsync();
        return new
        {
            a.Code, a.CustomerName, a.Phone, a.Vin, a.Plate, a.ServiceType, a.PreferredAt,
            a.DealerCode, a.Engineer, status = a.Status.ToString(), statusText = Text(a.Status),
            a.BayCode, a.AppTypeCode, a.SlotFrom, a.SlotTo, a.Note
        };
    }

    // ===== Phân công công việc sửa chữa (Ser_AssignmentWork) =====
    // Gắn 1 lệnh sửa chữa (ROID) với kế hoạch/thực tế theo từng công đoạn (SCC/SCD/SCN/SCS/SCDB/SCLR/SCKSC)
    // + danh sách KTV được phân công. Dedupe theo ROID (1 RO ↔ 1 phân công).
    public async Task<object> CreateWorkAssignmentAsync(CreateWorkAssignmentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RoId)) throw new InvalidOperationException("Cần RoId (mã lệnh sửa chữa).");
        var roId = dto.RoId.Trim().ToUpperInvariant();

        // MyCheck_Ser_RO: lệnh sửa chữa phải tồn tại (Ser_RO) trước khi phân công.
        var ro = await db.RepairOrders.FirstOrDefaultAsync(x => x.OrgId == Org && x.RoId == roId);
        if (ro is null) throw new InvalidOperationException($"Lệnh sửa chữa '{roId}' không tồn tại.");

        var stages = NormalizeStages(dto.Stages);
        // MyCheck_SerAssignmentWork_PlanDateTime_Cavity: chặn trùng khung giờ kế hoạch trên cùng khoang.
        await ValidateStageOverlapAsync(stages, excludeAssignmentId: null);

        var wa = await db.WorkAssignments.FirstOrDefaultAsync(x => x.OrgId == Org && x.RoId == roId);
        if (wa is null)
        {
            wa = new WorkAssignment { OrgId = Org, RoId = roId };
            db.WorkAssignments.Add(wa);
        }
        wa.RoNo = string.IsNullOrWhiteSpace(dto.RoNo) ? ro.RoNo : dto.RoNo!.Trim();
        wa.DealerCode = dto.DealerCode?.Trim() ?? ro.DealerCode;
        wa.WorkTypeStart = NormalizeWorkType(dto.WorkTypeStart);
        wa.WorkTypeFinish = NormalizeWorkType(dto.WorkTypeFinish);
        wa.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();   // cần Id cho các bảng con

        await ReplaceStagesAsync(wa, stages);
        await ReplaceEngineersAsync(wa, dto.Engineers);
        await db.SaveChangesAsync();
        return await BuildAssignmentViewAsync(wa);
    }

    // Danh sách phân công; lọc theo ROID, xưởng, ngày tạo.
    public async Task<object> ListWorkAssignmentsAsync(string? roId, string? dealer, string? date)
    {
        var q = db.WorkAssignments.Where(x => x.OrgId == Org);
        if (!string.IsNullOrWhiteSpace(roId)) q = q.Where(x => x.RoId == roId.Trim().ToUpperInvariant());
        if (!string.IsNullOrWhiteSpace(dealer)) q = q.Where(x => x.DealerCode == dealer);
        if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var d)) q = q.Where(x => x.CreatedAt.Date == d.Date);
        var list = await q.OrderByDescending(x => x.CreatedAt).Take(500).ToListAsync();
        var items = new List<object>();
        foreach (var wa in list) items.Add(await BuildAssignmentViewAsync(wa));
        return new { count = items.Count, items };
    }

    public async Task<object?> GetWorkAssignmentAsync(string roId)
    {
        roId = roId.Trim().ToUpperInvariant();
        var wa = await db.WorkAssignments.FirstOrDefaultAsync(x => x.OrgId == Org && x.RoId == roId);
        return wa is null ? null : await BuildAssignmentViewAsync(wa);
    }

    // Ser_AssignmentWork_UpdateDL: cập nhật kế hoạch/thực tế + thay danh sách KTV (delete olds → insert).
    public async Task<object?> UpdateWorkAssignmentAsync(string roId, UpdateWorkAssignmentDto dto)
    {
        roId = roId.Trim().ToUpperInvariant();
        var wa = await db.WorkAssignments.FirstOrDefaultAsync(x => x.OrgId == Org && x.RoId == roId);
        if (wa is null) return null;

        if (dto.Stages is not null)
        {
            var stages = NormalizeStages(dto.Stages);
            await ValidateStageOverlapAsync(stages, excludeAssignmentId: wa.Id);
            await ReplaceStagesAsync(wa, stages);
        }
        if (dto.Engineers is not null) await ReplaceEngineersAsync(wa, dto.Engineers);
        if (!string.IsNullOrWhiteSpace(dto.RoNo)) wa.RoNo = dto.RoNo!.Trim();
        if (!string.IsNullOrWhiteSpace(dto.DealerCode)) wa.DealerCode = dto.DealerCode!.Trim();
        if (dto.WorkTypeStart is not null) wa.WorkTypeStart = NormalizeWorkType(dto.WorkTypeStart);
        if (dto.WorkTypeFinish is not null) wa.WorkTypeFinish = NormalizeWorkType(dto.WorkTypeFinish);
        wa.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return await BuildAssignmentViewAsync(wa);
    }

    public async Task<object?> DeleteWorkAssignmentAsync(string roId)
    {
        roId = roId.Trim().ToUpperInvariant();
        var wa = await db.WorkAssignments.FirstOrDefaultAsync(x => x.OrgId == Org && x.RoId == roId);
        if (wa is null) return null;
        var stages = await db.WorkAssignmentStages.Where(x => x.OrgId == Org && x.AssignmentId == wa.Id).ToListAsync();
        var engs = await db.WorkAssignmentEngineers.Where(x => x.OrgId == Org && x.AssignmentId == wa.Id).ToListAsync();
        db.WorkAssignmentStages.RemoveRange(stages);
        db.WorkAssignmentEngineers.RemoveRange(engs);
        db.WorkAssignments.Remove(wa);
        await db.SaveChangesAsync();
        return new { roId, deleted = true };
    }

    // Chuẩn hóa danh sách công đoạn: chỉ nhận mã hợp lệ (WorkStages.All), dedupe theo WorkType.
    private static List<WorkStageDto> NormalizeStages(List<WorkStageDto>? stages)
    {
        var result = new List<WorkStageDto>();
        if (stages is null) return result;
        var seen = new HashSet<string>();
        foreach (var s in stages)
        {
            var wt = NormalizeWorkType(s.WorkType);
            if (wt is null || !seen.Add(wt)) continue;
            result.Add(s with { WorkType = wt });
        }
        return result;
    }

    private static string? NormalizeWorkType(string? code)
    {
        var c = (code ?? "").Trim().ToUpperInvariant();
        return WorkStages.All.Contains(c) ? c : null;
    }

    // MyCheck_SerAssignmentWork_PlanDateTime_Cavity: 2 công đoạn cùng khoang không được trùng khung giờ kế hoạch.
    private async Task ValidateStageOverlapAsync(List<WorkStageDto> stages, long? excludeAssignmentId)
    {
        var planned = stages.Where(s => !string.IsNullOrWhiteSpace(s.CavityCode) && s.PlanStart.HasValue && s.PlanFinish.HasValue).ToList();
        if (planned.Count == 0) return;

        // Kiểm tra trùng trong chính request (cùng khoang, khác công đoạn).
        for (int i = 0; i < planned.Count; i++)
            for (int j = i + 1; j < planned.Count; j++)
            {
                var a = planned[i]; var b = planned[j];
                if (string.Equals(a.CavityCode!.Trim(), b.CavityCode!.Trim(), StringComparison.OrdinalIgnoreCase)
                    && a.PlanStart!.Value < b.PlanFinish!.Value && a.PlanFinish!.Value > b.PlanStart!.Value)
                    throw new InvalidOperationException($"Khoang '{a.CavityCode}' bị trùng khung giờ kế hoạch giữa {a.WorkType} và {b.WorkType}.");
            }

        // Kiểm tra trùng với các phân công khác đã lưu (cùng khoang, giao khung giờ kế hoạch).
        var cavityCodes = planned.Select(s => s.CavityCode!.Trim().ToUpperInvariant()).Distinct().ToArray();
        var others = await db.WorkAssignmentStages
            .Where(x => x.OrgId == Org && x.CavityCode != null && cavityCodes.Contains(x.CavityCode)
                && x.PlanStart != null && x.PlanFinish != null
                && (excludeAssignmentId == null || x.AssignmentId != excludeAssignmentId))
            .ToListAsync();
        foreach (var s in planned)
        {
            var cav = s.CavityCode!.Trim().ToUpperInvariant();
            var conflict = others.FirstOrDefault(o => string.Equals(o.CavityCode, cav, StringComparison.OrdinalIgnoreCase)
                && o.PlanStart!.Value < s.PlanFinish!.Value && o.PlanFinish!.Value > s.PlanStart!.Value);
            if (conflict is not null)
                throw new InvalidOperationException($"Khoang '{s.CavityCode}' đã có công đoạn {conflict.WorkType} trùng khung giờ kế hoạch {s.PlanStart:HH:mm}-{s.PlanFinish:HH:mm}.");
        }
    }

    // Thay toàn bộ công đoạn của 1 phân công (delete olds → insert).
    private async Task ReplaceStagesAsync(WorkAssignment wa, List<WorkStageDto> stages)
    {
        var olds = await db.WorkAssignmentStages.Where(x => x.OrgId == Org && x.AssignmentId == wa.Id).ToListAsync();
        db.WorkAssignmentStages.RemoveRange(olds);
        foreach (var s in stages)
            db.WorkAssignmentStages.Add(new WorkAssignmentStage
            {
                OrgId = Org, AssignmentId = wa.Id, WorkType = s.WorkType,
                CavityCode = string.IsNullOrWhiteSpace(s.CavityCode) ? null : s.CavityCode!.Trim().ToUpperInvariant(),
                PlanStart = s.PlanStart, PlanFinish = s.PlanFinish, ActualStart = s.ActualStart, ActualFinish = s.ActualFinish
            });
    }

    // Thay toàn bộ KTV được phân công (delete olds → insert); WorkType mặc định SCC (sửa chữa chung).
    private async Task ReplaceEngineersAsync(WorkAssignment wa, List<WorkEngineerDto>? engineers)
    {
        var olds = await db.WorkAssignmentEngineers.Where(x => x.OrgId == Org && x.AssignmentId == wa.Id).ToListAsync();
        db.WorkAssignmentEngineers.RemoveRange(olds);
        if (engineers is null) return;
        var seen = new HashSet<string>();
        foreach (var e in engineers)
        {
            if (string.IsNullOrWhiteSpace(e.EngineerCode)) continue;
            var code = e.EngineerCode.Trim().ToUpperInvariant();
            var wt = NormalizeWorkType(e.WorkType) ?? WorkStages.SCC;
            if (!seen.Add(code + "|" + wt)) continue;
            db.WorkAssignmentEngineers.Add(new WorkAssignmentEngineer
            {
                OrgId = Org, AssignmentId = wa.Id, RoId = wa.RoId, EngineerCode = code, WorkType = wt
            });
        }
    }

    // Dựng view 1 phân công: thông tin RO + công đoạn (kèm tên) + KTV + tổng giờ kế hoạch.
    private async Task<object> BuildAssignmentViewAsync(WorkAssignment wa)
    {
        var stages = await db.WorkAssignmentStages.Where(x => x.OrgId == Org && x.AssignmentId == wa.Id)
            .OrderBy(x => x.WorkType).ToListAsync();
        var engs = await db.WorkAssignmentEngineers.Where(x => x.OrgId == Org && x.AssignmentId == wa.Id)
            .OrderBy(x => x.EngineerCode).ToListAsync();
        var stageViews = stages.Select(s => new
        {
            s.WorkType, workTypeText = WorkStages.Text(s.WorkType), s.CavityCode,
            s.PlanStart, s.PlanFinish, s.ActualStart, s.ActualFinish,
            planHours = s.PlanStart.HasValue && s.PlanFinish.HasValue ? Math.Round((s.PlanFinish.Value - s.PlanStart.Value).TotalHours, 2) : 0d
        });
        var totalPlanHours = Math.Round(stages.Where(s => s.PlanStart.HasValue && s.PlanFinish.HasValue)
            .Sum(s => (s.PlanFinish!.Value - s.PlanStart!.Value).TotalHours), 2);
        return new
        {
            wa.Id, wa.RoId, wa.RoNo, wa.DealerCode, wa.WorkTypeStart, wa.WorkTypeFinish,
            wa.CreatedAt, wa.UpdatedAt,
            stageCount = stages.Count, totalPlanHours,
            stages = stageViews,
            engineers = engs.Select(e => new { e.EngineerCode, e.WorkType, workTypeText = WorkStages.Text(e.WorkType) })
        };
    }
}
