using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MiniBooking.Data;
using MiniBooking.Models;
using MiniBooking.Services;
using Serilog;

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
FleetObs.ConfigureLogger("minibooking");

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
builder.WebHost.UseUrls($"http://0.0.0.0:{Environment.GetEnvironmentVariable("PORT") ?? "8080"}");

var conn = Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=minibooking.db";
builder.Services.AddDbContext<AppDbContext>(o =>
{
    if (DbUtil.IsPostgres(conn)) o.UseNpgsql(DbUtil.ToNpgsql(conn));
    else o.UseSqlite(conn);
});
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<IBookingService, BookingService>();

var ssoAuthority = Environment.GetEnvironmentVariable("SSO_AUTHORITY") ?? "https://minisso.onrender.com";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.Authority = ssoAuthority;
    o.RequireHttpsMetadata = ssoAuthority.StartsWith("https");
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = ssoAuthority,
        ValidateAudience = false, ValidateLifetime = true, NameClaimType = "name", RoleClaimType = "role"
    };
});
builder.Services.AddAuthorization();
builder.Services.AddFleetObs();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
    await Seeder.SeedAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>());

app.UseFleetObs();
FleetObs.ReportLicense(ssoAuthority, "minibooking");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/whoami", (ClaimsPrincipal u) => Results.Ok(new
{
    app = "minibooking",
    sub = u.FindFirst("sub")?.Value, name = u.Identity?.Name ?? u.FindFirst("name")?.Value,
    email = u.FindFirst("email")?.Value, tenant = u.FindFirst("tenant")?.Value,
    roles = u.FindAll("role").Select(c => c.Value)
})).RequireAuthorization();

app.Use(async (ctx, next) =>
{
    var key = ctx.Request.Headers["X-Api-Key"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(key)) ctx.Request.Cookies.TryGetValue(TenantContext.CookieName, out key);
    if (!string.IsNullOrWhiteSpace(key))
    {
        using var lookup = app.Services.CreateScope();
        var ldb = lookup.ServiceProvider.GetRequiredService<AppDbContext>();
        var org = await ldb.Orgs.FirstOrDefaultAsync(o => o.ApiKey == key);
        if (org != null) ctx.RequestServices.GetRequiredService<ITenantContext>().OrgId = org.Id;
    }
    await next();
});

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapGet("/healthz", () => "ok");

// ---- Công khai: khách đặt lịch + tra trạng thái ----
app.MapPost("/api/book", async (BookDto dto, IBookingService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.CustomerName) || string.IsNullOrWhiteSpace(dto.Phone))
        return Results.BadRequest(new { error = "Cần CustomerName và Phone." });
    if (dto.PreferredAt == default) return Results.BadRequest(new { error = "Cần PreferredAt (thời gian mong muốn)." });
    try { return Results.Ok(await svc.BookAsync(dto)); }
    catch (InvalidOperationException ex) { return Results.Conflict(new { error = ex.Message }); }
});

app.MapGet("/api/book/{code}", async (string code, IBookingService svc) =>
{
    var r = await svc.StatusAsync(code);
    return r is null ? Results.NotFound(new { code, found = false }) : Results.Ok(r);
});

// Ser_App_GetDL / SerAppController.GetByAppIdDL: chi tiết 1 lịch hẹn — header (khách/xe/khoang/KTV/RO)
// + danh sách dịch vụ (Ser_AppServiceItems) + phụ tùng (Ser_AppPartItems) trong 1 lần gọi.
app.MapGet("/api/appointments/{code}/detail", async (string code, IBookingService svc) =>
{
    var r = await svc.GetAppointmentDetailAsync(code);
    return r is null ? Results.NotFound(new { code, found = false }) : Results.Ok(r);
}).RequireAuthorization();

// ---- Nội bộ (SSO): quản lý lịch hẹn ----
app.MapGet("/api/appointments", async (IBookingService svc, string? status, string? dealer, string? date) =>
    Results.Ok(await svc.ListAsync(status, dealer, date))).RequireAuthorization();

// Ser_App_GetStatusList01DL: tìm kiếm nâng cao lịch hẹn (đa giá trị '|', mẫu biển số, tên KH, loại cuộc hẹn, timeline, phân trang).
app.MapGet("/api/appointments/search", async (IBookingService svc, string? dealerCodes, string? statuses, string? platePattern,
    string? customerName, string? creator, string? appTypeCodes, string? dateFrom, string? dateTimeline, int? recordStart, int? recordCount) =>
    Results.Ok(await svc.SearchAppointmentsAsync(new SearchAppointmentsDto(
        dealerCodes, statuses, platePattern, customerName, creator, appTypeCodes, dateFrom, dateTimeline, recordStart, recordCount)))
).RequireAuthorization();

// Ser_App_GetNewDL: tìm lịch hẹn theo bộ lọc "GetNew" (AppId/AppNo/CreatedDate/Creator/AppStatus/AppDateTime/PlateNo/CusName/DealerCode)
// + phân trang + tùy chọn mở rộng chi tiết (kèm dịch vụ & phụ tùng ngay trong kết quả).
app.MapGet("/api/appointments/get-new", async (IBookingService svc, string? appIds, string? dealerCodes, string? plateNos,
    string? appNos, string? customerNames, string? createdDates, string? appDateTimes, string? statuses, string? creators,
    bool? includeApp, bool? includeServiceItems, bool? includePartItems, int? recordStart, int? recordCount) =>
    Results.Ok(await svc.GetNewAppointmentsAsync(new GetNewAppointmentsDto(
        appIds, dealerCodes, plateNos, appNos, customerNames, createdDates, appDateTimes, statuses, creators,
        includeApp, includeServiceItems, includePartItems, recordStart, recordCount)))
).RequireAuthorization();

// Ser_App_GetForCavityDL: tìm lịch hẹn để xếp khoang — lọc theo biển số (chứa), 1 ngày cụ thể,
// và 4 cờ loại cuộc hẹn (BDDK bảo dưỡng định kỳ / SCC sửa chữa chung / SCDS sửa chữa đồng sơn / SCK sửa chữa khác).
app.MapGet("/api/appointments/for-cavity", async (IBookingService svc, string? plateNo, string? dateTimeLine,
    bool? flagBDDK, bool? flagSCC, bool? flagSCDS, bool? flagSCK) =>
    Results.Ok(await svc.GetForCavityAsync(new GetForCavityDto(plateNo, dateTimeLine, flagBDDK, flagSCC, flagSCDS, flagSCK)))
).RequireAuthorization();

// Ser_App_UpdateDL: sửa lịch hẹn đã có (đổi thời gian/khoang/loại/ghi chú + thay danh sách dịch vụ & phụ tùng).
app.MapPut("/api/appointments/{code}", async (string code, UpdateAppointmentDto dto, IBookingService svc) =>
{
    try
    {
        var r = await svc.UpdateAppointmentAsync(code, dto);
        return r is null ? Results.NotFound(new { code, error = "Không thấy lịch hẹn hoặc lịch đã hoàn tất/đã hủy." }) : Results.Ok(r);
    }
    catch (InvalidOperationException ex) { return Results.Conflict(new { code, error = ex.Message }); }
}).RequireAuthorization();

// Gọi xác nhận lịch trước giờ hẹn (Ser_CustomerCare72h): Requested → Contacted (AppStatus=5).
app.MapPost("/api/appointments/{code}/contact", async (string code, ContactApptDto dto, IBookingService svc) =>
{
    var r = await svc.ContactAsync(code, dto);
    return r is null ? Results.NotFound(new { code, error = "Không thấy lịch chờ liên hệ." }) : Results.Ok(r);
}).RequireAuthorization();

// Danh sách lịch cần gọi xác nhận (còn Chờ xác nhận, giờ hẹn trong vòng N giờ tới).
app.MapGet("/api/appointments/due-for-contact", async (IBookingService svc, int? withinHours) =>
    Results.Ok(await svc.DueForContactAsync(withinHours ?? 24))).RequireAuthorization();

app.MapPost("/api/appointments/{code}/confirm", async (string code, ConfirmDto dto, IBookingService svc) =>
{
    try
    {
        var r = await svc.ConfirmAsync(code, dto);
        return r is null ? Results.NotFound(new { code, error = "Không thấy lịch chờ xác nhận." }) : Results.Ok(r);
    }
    catch (InvalidOperationException ex) { return Results.Conflict(new { code, error = ex.Message }); }
}).RequireAuthorization();

app.MapPost("/api/appointments/{code}/checkin", async (string code, CheckInDto dto, IBookingService svc) =>
{
    var r = await svc.CheckInAsync(code, dto.RoNo);
    return r is null ? Results.NotFound(new { code, error = "Không thấy lịch để tiếp nhận." }) : Results.Ok(r);
}).RequireAuthorization();

app.MapPost("/api/appointments/{code}/done", async (string code, IBookingService svc) =>
{
    var r = await svc.DoneAsync(code);
    return r is null ? Results.NotFound(new { code, error = "Lịch chưa check-in." }) : Results.Ok(r);
}).RequireAuthorization();

app.MapPost("/api/appointments/{code}/cancel", async (string code, IBookingService svc, bool? noShow) =>
{
    try
    {
        var r = await svc.CancelAsync(code, noShow ?? false);
        return r is null ? Results.NotFound(new { code, error = "Không hủy được (đã xong/đã hủy)." }) : Results.Ok(r);
    }
    catch (InvalidOperationException ex) { return Results.Conflict(new { code, error = ex.Message }); }
}).RequireAuthorization();

// Ser_App_UpdateStatusDL: đổi trạng thái lịch hẹn theo máy trạng thái AppStatus
// (1=Mới tạo, 2=Xác nhận, 3=Tiếp nhận, 4=Hủy, 5=Đã liên hệ & Chưa xác nhận) + ghi lịch sử.
app.MapPost("/api/appointments/{code}/status", async (string code, ChangeApptStatusDto dto, IBookingService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.ToStatus)) return Results.BadRequest(new { error = "Cần ToStatus." });
    try
    {
        var r = await svc.ChangeAppointmentStatusAsync(code, dto);
        return r is null ? Results.NotFound(new { code, error = "Không thấy lịch hẹn." }) : Results.Ok(r);
    }
    catch (InvalidOperationException ex) { return Results.Conflict(new { code, error = ex.Message }); }
}).RequireAuthorization();

// Lịch sử đổi trạng thái lịch hẹn (Ser_App_UpdateStatusDL).
app.MapGet("/api/appointments/{code}/status-history", async (string code, IBookingService svc) =>
{
    var r = await svc.GetAppointmentStatusHistoryAsync(code);
    return r is null ? Results.NotFound(new { code, found = false }) : Results.Ok(r);
}).RequireAuthorization();

app.MapGet("/api/calendar", async (string date, IBookingService svc, string? dealer) =>
    Results.Ok(await svc.CalendarAsync(date, dealer))).RequireAuthorization();
app.MapGet("/api/stats", async (IBookingService svc) => Results.Ok(await svc.StatsAsync())).RequireAuthorization();

// ---- Kỹ thuật viên (Ser_Engineer): roster + tải công việc ----
app.MapPost("/api/engineers", async (AddEngineerDto dto, IBookingService svc) =>
    string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name)
        ? Results.BadRequest(new { error = "Cần Code và Name." }) : Results.Ok(await svc.AddEngineerAsync(dto))).RequireAuthorization();

app.MapGet("/api/engineers/workload", async (string date, IBookingService svc, string? dealer) =>
    Results.Ok(await svc.EngineerWorkloadAsync(date, dealer))).RequireAuthorization();

// ---- Khoang sửa chữa (Ser_Cavity): master + sức chứa theo khung giờ ----
app.MapPost("/api/bays", async (AddBayDto dto, IBookingService svc) =>
    string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name)
        ? Results.BadRequest(new { error = "Cần Code và Name." }) : Results.Ok(await svc.AddBayAsync(dto))).RequireAuthorization();

app.MapGet("/api/bays", async (IBookingService svc, string? dealer) =>
    Results.Ok(await svc.ListBaysAsync(dealer))).RequireAuthorization();

app.MapGet("/api/bays/availability", async (string date, IBookingService svc, string? bayCode, string? dealer) =>
    Results.Ok(await svc.SlotAvailabilityAsync(date, bayCode, dealer))).RequireAuthorization();

// ---- Loại cuộc hẹn (Mst_Ser_AppType): master + danh sách ----
app.MapPost("/api/app-types", async (AddAppTypeDto dto, IBookingService svc) =>
    string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name)
        ? Results.BadRequest(new { error = "Cần Code và Name." }) : Results.Ok(await svc.AddAppTypeAsync(dto))).RequireAuthorization();

app.MapGet("/api/app-types", async (IBookingService svc, bool? active) =>
    Results.Ok(await svc.ListAppTypesAsync(active))).RequireAuthorization();

// ---- Loại khoang sửa chữa (Mst_Compartment / Ser_Cavity.CavityType): master + ràng buộc theo dịch vụ ----
app.MapPost("/api/cavity-types", async (AddCavityTypeDto dto, IBookingService svc) =>
    string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name)
        ? Results.BadRequest(new { error = "Cần Code và Name." }) : Results.Ok(await svc.AddCavityTypeAsync(dto))).RequireAuthorization();

app.MapGet("/api/cavity-types", async (IBookingService svc, bool? active) =>
    Results.Ok(await svc.ListCavityTypesAsync(active))).RequireAuthorization();

// ---- Tổ kỹ thuật (Ser_GroupRepair — "Quản lý tổ kỹ thuật"): master nhóm KTV theo xưởng ----
// Ser_GroupRepair_Create/Update: tạo/cập nhật (validate GroupRNo/DealerCode/GroupRName + mã tổ duy nhất theo đại lý).
app.MapPost("/api/repair-groups", async (SaveRepairGroupDto dto, IBookingService svc) =>
{
    try { return Results.Ok(await svc.SaveRepairGroupAsync(dto)); }
    catch (InvalidOperationException ex) { return Results.Conflict(new { error = ex.Message }); }
}).RequireAuthorization();

// Ser_GroupRepair_Get_DL: tìm tổ kỹ thuật (đại lý + từ khóa mã/tên + cờ hiệu lực + phân trang).
app.MapGet("/api/repair-groups", async (IBookingService svc, string? dealer, string? keyword, bool? active, int? recordStart, int? recordCount) =>
    Results.Ok(await svc.ListRepairGroupsAsync(dealer, keyword, active, recordStart, recordCount))).RequireAuthorization();

app.MapGet("/api/repair-groups/{id:long}", async (long id, IBookingService svc) =>
{
    var r = await svc.GetRepairGroupAsync(id);
    return r is null ? Results.NotFound(new { id, found = false }) : Results.Ok(r);
}).RequireAuthorization();

// Ser_GroupRepair_Delete: xóa tổ kỹ thuật.
app.MapDelete("/api/repair-groups/{id:long}", async (long id, IBookingService svc) =>
{
    var r = await svc.DeleteRepairGroupAsync(id);
    return r is null ? Results.NotFound(new { id, error = "Không thấy tổ kỹ thuật." }) : Results.Ok(r);
}).RequireAuthorization();

// ---- Lịch làm việc của xưởng (Mst_Calendar): ngày làm việc/nghỉ theo năm ----
// Mst_Calendar_ResetYear: khởi tạo lịch cả năm theo StatusValue từng thứ (0 = làm việc).
app.MapPost("/api/calendar-days/reset-year", async (ResetCalendarYearDto dto, IBookingService svc) =>
{
    try { return Results.Ok(await svc.ResetCalendarYearAsync(dto)); }
    catch (InvalidOperationException ex) { return Results.Conflict(new { error = ex.Message }); }
}).RequireAuthorization();

// Mst_Calendar_Get: danh sách ngày theo loại lịch + năm (hoặc khoảng from..to).
app.MapGet("/api/calendar-days", async (IBookingService svc, string? calendarType, int? year, string? from, string? to) =>
    Results.Ok(await svc.ListCalendarDaysAsync(calendarType, year, from, to))).RequireAuthorization();

// Mst_Calendar_UpdateStatusValue: đổi trạng thái làm việc/nghỉ của 1 ngày.
app.MapPut("/api/calendar-days", async (UpdateCalendarDayDto dto, IBookingService svc) =>
{
    try
    {
        var r = await svc.UpdateCalendarDayAsync(dto);
        return r is null ? Results.NotFound(new { dto.Date, error = "Không thấy ngày trong lịch." }) : Results.Ok(r);
    }
    catch (InvalidOperationException ex) { return Results.Conflict(new { error = ex.Message }); }
}).RequireAuthorization();

// Mst_Calendar_GetDateToCheck: ngày làm việc thứ N kể từ mốc 'from'.
app.MapGet("/api/calendar-days/next-working", async (IBookingService svc, string from, int? dayOffset) =>
{
    try
    {
        var r = await svc.NextWorkingDayAsync(from, dayOffset ?? 0);
        return r is null ? Results.NotFound(new { from, dayOffset = dayOffset ?? 0, error = "Không đủ ngày làm việc trong lịch đã khởi tạo." }) : Results.Ok(r);
    }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
}).RequireAuthorization();

// ---- Dịch vụ đăng ký kèm lịch hẹn (Ser_AppServiceItems): công việc + giờ công chuẩn ----
app.MapPost("/api/appointments/{code}/services", async (string code, AddServiceItemDto dto, IBookingService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.SerCode)) return Results.BadRequest(new { error = "Cần SerCode." });
    try
    {
        var r = await svc.AddServiceItemAsync(code, dto);
        return r is null ? Results.NotFound(new { code, error = "Không thấy lịch hẹn." }) : Results.Ok(r);
    }
    catch (InvalidOperationException ex) { return Results.Conflict(new { code, error = ex.Message }); }
}).RequireAuthorization();

app.MapGet("/api/appointments/{code}/services", async (string code, IBookingService svc) =>
{
    var r = await svc.ListServiceItemsAsync(code);
    return r is null ? Results.NotFound(new { code, error = "Không thấy lịch hẹn." }) : Results.Ok(r);
}).RequireAuthorization();

app.MapDelete("/api/appointments/{code}/services/{itemId:long}", async (string code, long itemId, IBookingService svc) =>
{
    var r = await svc.RemoveServiceItemAsync(code, itemId);
    return r is null ? Results.NotFound(new { code, itemId, error = "Không thấy dịch vụ của lịch hẹn." }) : Results.Ok(r);
}).RequireAuthorization();

// ---- Phụ tùng đăng ký kèm lịch hẹn (Ser_AppPartItems): phụ tùng + số lượng + tồn kho ----
app.MapPost("/api/appointments/{code}/parts", async (string code, AddPartItemDto dto, IBookingService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.PartCode)) return Results.BadRequest(new { error = "Cần PartCode." });
    try
    {
        var r = await svc.AddPartItemAsync(code, dto);
        return r is null ? Results.NotFound(new { code, error = "Không thấy lịch hẹn." }) : Results.Ok(r);
    }
    catch (InvalidOperationException ex) { return Results.Conflict(new { code, error = ex.Message }); }
}).RequireAuthorization();

app.MapGet("/api/appointments/{code}/parts", async (string code, IBookingService svc) =>
{
    var r = await svc.ListPartItemsAsync(code);
    return r is null ? Results.NotFound(new { code, error = "Không thấy lịch hẹn." }) : Results.Ok(r);
}).RequireAuthorization();

app.MapDelete("/api/appointments/{code}/parts/{itemId:long}", async (string code, long itemId, IBookingService svc) =>
{
    var r = await svc.RemovePartItemAsync(code, itemId);
    return r is null ? Results.NotFound(new { code, itemId, error = "Không thấy phụ tùng của lịch hẹn." }) : Results.Ok(r);
}).RequireAuthorization();

// ---- Lệnh sửa chữa / báo giá (Ser_RO): nguồn để đặt lịch hẹn + gắn RO ↔ lịch hẹn ----
app.MapPost("/api/repair-orders", async (AddRepairOrderDto dto, IBookingService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.RoId)) return Results.BadRequest(new { error = "Cần RoId." });
    try { return Results.Ok(await svc.AddRepairOrderAsync(dto)); }
    catch (InvalidOperationException ex) { return Results.Conflict(new { error = ex.Message }); }
}).RequireAuthorization();

app.MapGet("/api/repair-orders", async (IBookingService svc, string? dealer, bool? linked) =>
    Results.Ok(await svc.ListRepairOrdersAsync(dealer, linked))).RequireAuthorization();

app.MapGet("/api/repair-orders/{roId}", async (string roId, IBookingService svc) =>
{
    var r = await svc.GetRepairOrderAsync(roId);
    return r is null ? Results.NotFound(new { roId, found = false }) : Results.Ok(r);
}).RequireAuthorization();

// Ser_RO_UpdateAppId: gắn lệnh sửa chữa với lịch hẹn (đặt AppId cho RO, ROID cho lịch hẹn).
app.MapPost("/api/repair-orders/{roId}/link", async (string roId, LinkRepairOrderDto dto, IBookingService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.AppCode)) return Results.BadRequest(new { error = "Cần AppCode." });
    try
    {
        var r = await svc.LinkRepairOrderAsync(roId, dto.AppCode);
        return r is null ? Results.NotFound(new { roId, error = "Không thấy lệnh sửa chữa hoặc lịch hẹn." }) : Results.Ok(r);
    }
    catch (InvalidOperationException ex) { return Results.Conflict(new { roId, error = ex.Message }); }
}).RequireAuthorization();

// Ser_RO_UpdateStatus: chuyển trạng thái lệnh sửa chữa theo máy trạng thái Ser_RO_Stage
// (CRE→PRT→W4P→HPA→HRO→INGA→RPRD→CEND→PAID→FNS; nhánh REJ/NORE).
app.MapPost("/api/repair-orders/{roId}/status", async (string roId, ChangeRoStatusDto dto, IBookingService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.ToStatus)) return Results.BadRequest(new { error = "Cần ToStatus." });
    try
    {
        var r = await svc.ChangeRepairOrderStatusAsync(roId, dto);
        return r is null ? Results.NotFound(new { roId, error = "Không thấy lệnh sửa chữa." }) : Results.Ok(r);
    }
    catch (InvalidOperationException ex) { return Results.Conflict(new { roId, error = ex.Message }); }
}).RequireAuthorization();

// Lịch sử đổi trạng thái lệnh sửa chữa (Ser_ROHistory).
app.MapGet("/api/repair-orders/{roId}/status-history", async (string roId, IBookingService svc) =>
{
    var r = await svc.GetRepairOrderStatusHistoryAsync(roId);
    return r is null ? Results.NotFound(new { roId, found = false }) : Results.Ok(r);
}).RequireAuthorization();

// SerROToRORejectStatusDL: hủy/từ chối lệnh sửa chữa (bắt buộc RejectDate + RejectNote;
// chặn khi RO đã RPRD/PAID/FNS/CEND; ghi lịch sử REJ + xóa phân công công việc của RO).
app.MapPost("/api/repair-orders/{roId}/reject", async (string roId, RejectRepairOrderDto dto, IBookingService svc) =>
{
    try
    {
        var r = await svc.RejectRepairOrderAsync(roId, dto);
        return r is null ? Results.NotFound(new { roId, error = "Không thấy lệnh sửa chữa." }) : Results.Ok(r);
    }
    catch (InvalidOperationException ex) { return Results.Conflict(new { roId, error = ex.Message }); }
}).RequireAuthorization();

// Ser_RO_UpdatePlanedDeliveryDateDL: lưu ngày giao xe dự kiến của lệnh sửa chữa (kèm lý do).
app.MapPost("/api/repair-orders/{roId}/planned-delivery-date", async (string roId, UpdatePlannedDeliveryDateDto dto, IBookingService svc) =>
{
    try
    {
        var r = await svc.UpdatePlannedDeliveryDateAsync(roId, dto);
        return r is null ? Results.NotFound(new { roId, error = "Không thấy lệnh sửa chữa." }) : Results.Ok(r);
    }
    catch (InvalidOperationException ex) { return Results.Conflict(new { roId, error = ex.Message }); }
}).RequireAuthorization();

// Lịch sử ngày giao xe dự kiến của lệnh sửa chữa (Ser_Ro_PlanedDeliveryDate_His).
app.MapGet("/api/repair-orders/{roId}/planned-delivery-date", async (string roId, IBookingService svc) =>
{
    var r = await svc.GetPlannedDeliveryDateHistoryAsync(roId);
    return r is null ? Results.NotFound(new { roId, found = false }) : Results.Ok(r);
}).RequireAuthorization();

// Ser_RO_Update_Maintance_DL: cập nhật thông tin nhắc bảo dưỡng kế tiếp của lệnh sửa chữa
// (Km hiện tại + ngày/mốc Km nhắc bảo dưỡng + công việc cần làm sớm + mã hội viên).
app.MapPost("/api/repair-orders/{roId}/maintenance", async (string roId, UpdateRoMaintenanceDto dto, IBookingService svc) =>
{
    try
    {
        var r = await svc.UpdateRoMaintenanceAsync(roId, dto);
        return r is null ? Results.NotFound(new { roId, error = "Không thấy lệnh sửa chữa." }) : Results.Ok(r);
    }
    catch (InvalidOperationException ex) { return Results.Conflict(new { roId, error = ex.Message }); }
}).RequireAuthorization();

// Ser_RO_Sumary_DL: thống kê lệnh sửa chữa theo ngày (lọc đại lý '|', khoảng ngày CheckInDate, trạng thái '|')
// + doanh thu mỗi RO = Σ phụ tùng + Σ công việc (Price*Qty*Factor*(1+VAT/100)).
app.MapGet("/api/repair-orders/summary", async (IBookingService svc, string? dealerCodes, string? fromDate, string? toDate, string? statuses) =>
    Results.Ok(await svc.SummarizeRepairOrdersAsync(new RoSummaryDto(dealerCodes, fromDate, toDate, statuses)))
).RequireAuthorization();

// Ser_RO_GetForSerAppDL: lấy dữ liệu lệnh sửa chữa để tạo lịch hẹn (header RO + công việc + phụ tùng cần).
app.MapGet("/api/repair-orders/{roId}/for-appointment", async (string roId, IBookingService svc) =>
{
    var r = await svc.GetRepairOrderForAppointmentAsync(roId);
    return r is null ? Results.NotFound(new { roId, found = false }) : Results.Ok(r);
}).RequireAuthorization();

// ---- Công việc trong lệnh sửa chữa (Ser_ROServiceItems): dòng công việc + trạng thái hoàn thành ----
app.MapPost("/api/repair-orders/{roId}/services", async (string roId, AddRoServiceItemDto dto, IBookingService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.SerCode)) return Results.BadRequest(new { error = "Cần SerCode." });
    try
    {
        var r = await svc.AddRoServiceItemAsync(roId, dto);
        return r is null ? Results.NotFound(new { roId, error = "Không thấy lệnh sửa chữa." }) : Results.Ok(r);
    }
    catch (InvalidOperationException ex) { return Results.Conflict(new { roId, error = ex.Message }); }
}).RequireAuthorization();

app.MapGet("/api/repair-orders/{roId}/services", async (string roId, IBookingService svc) =>
{
    var r = await svc.ListRoServiceItemsAsync(roId);
    return r is null ? Results.NotFound(new { roId, error = "Không thấy lệnh sửa chữa." }) : Results.Ok(r);
}).RequireAuthorization();

app.MapDelete("/api/repair-orders/{roId}/services/{itemId:long}", async (string roId, long itemId, IBookingService svc) =>
{
    var r = await svc.RemoveRoServiceItemAsync(roId, itemId);
    return r is null ? Results.NotFound(new { roId, itemId, error = "Không thấy công việc của lệnh sửa chữa." }) : Results.Ok(r);
}).RequireAuthorization();

// Ser_RO_Update_ServiceItemsStatusRODL: cập nhật trạng thái xong của các dòng công việc;
// khi mọi dòng đã xong → Ser_RO.ServiceStatus = Active.
app.MapPost("/api/repair-orders/{roId}/services/status", async (string roId, UpdateRoServiceItemsStatusDto dto, IBookingService svc) =>
{
    try
    {
        var r = await svc.UpdateRoServiceItemsStatusAsync(roId, dto);
        return r is null ? Results.NotFound(new { roId, error = "Không thấy lệnh sửa chữa." }) : Results.Ok(r);
    }
    catch (InvalidOperationException ex) { return Results.Conflict(new { roId, error = ex.Message }); }
}).RequireAuthorization();

// ---- Chăm sóc KH dịch vụ (Ser_CustomerCare): nhắc bảo dưỡng/sinh nhật → liên hệ → đặt lịch ----
app.MapPost("/api/care", async (CreateReminderDto dto, IBookingService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.CustomerName) || string.IsNullOrWhiteSpace(dto.Phone))
        return Results.BadRequest(new { error = "Cần CustomerName và Phone." });
    return Results.Ok(await svc.CreateReminderAsync(dto));
}).RequireAuthorization();

app.MapGet("/api/care", async (IBookingService svc, string? status, string? careType, string? dueBefore) =>
    Results.Ok(await svc.ListRemindersAsync(status, careType, dueBefore))).RequireAuthorization();

app.MapPost("/api/care/{id:long}/contact", async (long id, ContactDto dto, IBookingService svc) =>
{
    var r = await svc.ContactReminderAsync(id, dto.Note);
    return r is null ? Results.NotFound(new { id, error = "Không thấy nhắc hoặc đã đặt lịch/đóng." }) : Results.Ok(r);
}).RequireAuthorization();

app.MapPost("/api/care/{id:long}/convert", async (long id, ConvertDto dto, IBookingService svc) =>
{
    if (dto.PreferredAt == default) return Results.BadRequest(new { error = "Cần PreferredAt." });
    var r = await svc.ConvertReminderAsync(id, dto);
    return r is null ? Results.NotFound(new { id, error = "Không thấy nhắc hoặc đã đặt lịch." }) : Results.Ok(r);
}).RequireAuthorization();

app.MapGet("/api/care-stats", async (IBookingService svc) => Results.Ok(await svc.CareStatsAsync())).RequireAuthorization();

// ---- Nhắc chăm sóc sinh nhật KH (Ser_CustomerCareBth): mỗi khách 1 dòng nhắc sinh nhật → gọi chúc mừng → cập nhật trạng thái ----
app.MapPost("/api/birthday-care", async (CreateBirthdayCareDto dto, IBookingService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.CusId) || string.IsNullOrWhiteSpace(dto.CustomerName))
        return Results.BadRequest(new { error = "Cần CusId và CustomerName." });
    return Results.Ok(await svc.CreateBirthdayCareAsync(dto));
}).RequireAuthorization();

// Ser_CustomerCareBth_Get_DL: tìm nhắc sinh nhật (trạng thái/đại lý/tên KH/biển số/số khung/ngày sinh + phân trang).
app.MapGet("/api/birthday-care", async (IBookingService svc, string? status, string? dealer, string? customerName,
    string? plate, string? frameNo, string? dateBth, int? recordStart, int? recordCount) =>
    Results.Ok(await svc.ListBirthdayCaresAsync(status, dealer, customerName, plate, frameNo, dateBth, recordStart, recordCount))).RequireAuthorization();

app.MapGet("/api/birthday-care/{careBthId}", async (string careBthId, IBookingService svc) =>
{
    var r = await svc.GetBirthdayCareAsync(careBthId);
    return r is null ? Results.NotFound(new { careBthId, found = false }) : Results.Ok(r);
}).RequireAuthorization();

// Ser_CustomerCareBth_Update: cập nhật trạng thái liên hệ (0/1/2) + ngày liên hệ + ghi chú + ngày sinh.
app.MapPut("/api/birthday-care/{careBthId}", async (string careBthId, UpdateBirthdayCareDto dto, IBookingService svc) =>
{
    try
    {
        var r = await svc.UpdateBirthdayCareAsync(careBthId, dto);
        return r is null ? Results.NotFound(new { careBthId, error = "Không thấy nhắc sinh nhật." }) : Results.Ok(r);
    }
    catch (InvalidOperationException ex) { return Results.Conflict(new { careBthId, error = ex.Message }); }
}).RequireAuthorization();

app.MapGet("/api/birthday-care-stats", async (IBookingService svc) => Results.Ok(await svc.BirthdayCareStatsAsync())).RequireAuthorization();

// ---- Chăm sóc KH sau dịch vụ 72h (Ser_CustomerCare72h): khảo sát hài lòng sau khi giao xe ----
app.MapPost("/api/post-care", async (CreatePostCareDto dto, IBookingService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.CusCareId) || string.IsNullOrWhiteSpace(dto.CustomerName))
        return Results.BadRequest(new { error = "Cần CusCareId và CustomerName." });
    try { return Results.Ok(await svc.CreatePostCareAsync(dto)); }
    catch (InvalidOperationException ex) { return Results.Conflict(new { error = ex.Message }); }
}).RequireAuthorization();

app.MapGet("/api/post-care", async (IBookingService svc, string? status, string? dealer, string? dueBefore) =>
    Results.Ok(await svc.ListPostCaresAsync(status, dealer, dueBefore))).RequireAuthorization();

app.MapGet("/api/post-care/{cusCareId}", async (string cusCareId, IBookingService svc) =>
{
    var r = await svc.GetPostCareAsync(cusCareId);
    return r is null ? Results.NotFound(new { cusCareId, found = false }) : Results.Ok(r);
}).RequireAuthorization();

// Ghi nhận liên hệ + trả lời khảo sát (PEND → CINFB/CIFB).
app.MapPost("/api/post-care/{cusCareId}/contact", async (string cusCareId, PostCareContactDto dto, IBookingService svc) =>
{
    var r = await svc.ContactPostCareAsync(cusCareId, dto);
    return r is null ? Results.NotFound(new { cusCareId, error = "Không thấy phiếu chăm sóc hoặc đã bỏ qua." }) : Results.Ok(r);
}).RequireAuthorization();

// Bỏ qua không cần liên hệ (REJ).
app.MapPost("/api/post-care/{cusCareId}/reject", async (string cusCareId, IBookingService svc, string? note) =>
{
    var r = await svc.RejectPostCareAsync(cusCareId, note);
    return r is null ? Results.NotFound(new { cusCareId, error = "Không thấy phiếu chăm sóc." }) : Results.Ok(r);
}).RequireAuthorization();

app.MapGet("/api/post-care-stats", async (IBookingService svc) => Results.Ok(await svc.PostCareStatsAsync())).RequireAuthorization();

// ---- Chăm sóc KH sau dịch vụ 24h (Ser_CustomerCare24h): khảo sát hài lòng sớm sau khi giao xe ----
app.MapPost("/api/post-care-24h", async (CreatePostCare24hDto dto, IBookingService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.CusCareId) || string.IsNullOrWhiteSpace(dto.CustomerName))
        return Results.BadRequest(new { error = "Cần CusCareId và CustomerName." });
    try { return Results.Ok(await svc.CreatePostCare24hAsync(dto)); }
    catch (InvalidOperationException ex) { return Results.Conflict(new { error = ex.Message }); }
}).RequireAuthorization();

app.MapGet("/api/post-care-24h", async (IBookingService svc, string? status, string? dealer, string? dueBefore,
    string? customerName, string? plate, string? frameNo) =>
    Results.Ok(await svc.ListPostCares24hAsync(status, dealer, dueBefore, customerName, plate, frameNo))).RequireAuthorization();

app.MapGet("/api/post-care-24h/{cusCareId}", async (string cusCareId, IBookingService svc) =>
{
    var r = await svc.GetPostCare24hAsync(cusCareId);
    return r is null ? Results.NotFound(new { cusCareId, found = false }) : Results.Ok(r);
}).RequireAuthorization();

// Ghi nhận liên hệ + trả lời khảo sát (PEND → CINFB/CIFB).
app.MapPost("/api/post-care-24h/{cusCareId}/contact", async (string cusCareId, PostCare24hContactDto dto, IBookingService svc) =>
{
    var r = await svc.ContactPostCare24hAsync(cusCareId, dto);
    return r is null ? Results.NotFound(new { cusCareId, error = "Không thấy phiếu chăm sóc 24h hoặc đã bỏ qua." }) : Results.Ok(r);
}).RequireAuthorization();

// Bỏ qua không cần liên hệ (REJ).
app.MapPost("/api/post-care-24h/{cusCareId}/reject", async (string cusCareId, IBookingService svc, string? note) =>
{
    var r = await svc.RejectPostCare24hAsync(cusCareId, note);
    return r is null ? Results.NotFound(new { cusCareId, error = "Không thấy phiếu chăm sóc 24h." }) : Results.Ok(r);
}).RequireAuthorization();

app.MapGet("/api/post-care-24h-stats", async (IBookingService svc) => Results.Ok(await svc.PostCare24hStatsAsync())).RequireAuthorization();

// ---- Phiếu tiếp nhận xe (Ser_ReceptionF): lập khi khách đến xưởng → giao xe (P → A) ----
app.MapPost("/api/reception-forms", async (CreateReceptionFormDto dto, IBookingService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.CustomerName)) return Results.BadRequest(new { error = "Cần CustomerName." });
    return Results.Ok(await svc.CreateReceptionFormAsync(dto));
}).RequireAuthorization();

app.MapGet("/api/reception-forms", async (IBookingService svc, string? status, string? dealer, string? date) =>
    Results.Ok(await svc.ListReceptionFormsAsync(status, dealer, date))).RequireAuthorization();

app.MapGet("/api/reception-forms/{receptionFNo}", async (string receptionFNo, IBookingService svc) =>
{
    var r = await svc.GetReceptionFormAsync(receptionFNo);
    return r is null ? Results.NotFound(new { receptionFNo, found = false }) : Results.Ok(r);
}).RequireAuthorization();

// Giao xe: P (Tiếp nhận) → A (Giao xe).
app.MapPost("/api/reception-forms/{receptionFNo}/deliver", async (string receptionFNo, DeliverReceptionFormDto dto, IBookingService svc) =>
{
    var r = await svc.DeliverReceptionFormAsync(receptionFNo, dto);
    return r is null ? Results.NotFound(new { receptionFNo, error = "Không thấy phiếu hoặc đã giao xe." }) : Results.Ok(r);
}).RequireAuthorization();

// Ser_ReceptionF_DeleteX_ExistRONotDelete: chặn xóa khi phiếu đã phát sinh RO.
app.MapDelete("/api/reception-forms/{receptionFNo}", async (string receptionFNo, IBookingService svc) =>
{
    try
    {
        var r = await svc.DeleteReceptionFormAsync(receptionFNo);
        return r is null ? Results.NotFound(new { receptionFNo, error = "Không thấy phiếu tiếp nhận." }) : Results.Ok(r);
    }
    catch (InvalidOperationException ex) { return Results.Conflict(new { receptionFNo, error = ex.Message }); }
}).RequireAuthorization();

// ---- Phân công công việc sửa chữa (Ser_AssignmentWork): gắn RO ↔ công đoạn (kế hoạch/thực tế) + KTV ----
app.MapPost("/api/work-assignments", async (CreateWorkAssignmentDto dto, IBookingService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.RoId)) return Results.BadRequest(new { error = "Cần RoId." });
    try { return Results.Ok(await svc.CreateWorkAssignmentAsync(dto)); }
    catch (InvalidOperationException ex) { return Results.Conflict(new { error = ex.Message }); }
}).RequireAuthorization();

app.MapGet("/api/work-assignments", async (IBookingService svc, string? roId, string? dealer, string? date) =>
    Results.Ok(await svc.ListWorkAssignmentsAsync(roId, dealer, date))).RequireAuthorization();

app.MapGet("/api/work-assignments/{roId}", async (string roId, IBookingService svc) =>
{
    var r = await svc.GetWorkAssignmentAsync(roId);
    return r is null ? Results.NotFound(new { roId, found = false }) : Results.Ok(r);
}).RequireAuthorization();

app.MapPut("/api/work-assignments/{roId}", async (string roId, UpdateWorkAssignmentDto dto, IBookingService svc) =>
{
    try
    {
        var r = await svc.UpdateWorkAssignmentAsync(roId, dto);
        return r is null ? Results.NotFound(new { roId, error = "Không thấy phân công công việc." }) : Results.Ok(r);
    }
    catch (InvalidOperationException ex) { return Results.Conflict(new { roId, error = ex.Message }); }
}).RequireAuthorization();

app.MapDelete("/api/work-assignments/{roId}", async (string roId, IBookingService svc) =>
{
    var r = await svc.DeleteWorkAssignmentAsync(roId);
    return r is null ? Results.NotFound(new { roId, error = "Không thấy phân công công việc." }) : Results.Ok(r);
}).RequireAuthorization();

// ---- Gói dịch vụ (Ser_ServicePackage): nhóm sẵn công việc + phụ tùng theo 1 giá gói ----
app.MapPost("/api/service-packages", async (CreateServicePackageDto dto, IBookingService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.PackageNo) || string.IsNullOrWhiteSpace(dto.PackageName) || string.IsNullOrWhiteSpace(dto.DealerCode))
        return Results.BadRequest(new { error = "Cần PackageNo, PackageName và DealerCode." });
    try { return Results.Ok(await svc.CreateServicePackageAsync(dto)); }
    catch (InvalidOperationException ex) { return Results.Conflict(new { error = ex.Message }); }
}).RequireAuthorization();

app.MapGet("/api/service-packages", async (IBookingService svc, string? dealer, string? keyword, bool? isPublic) =>
    Results.Ok(await svc.ListServicePackagesAsync(dealer, keyword, isPublic))).RequireAuthorization();

app.MapGet("/api/service-packages/{id:long}", async (long id, IBookingService svc) =>
{
    var r = await svc.GetServicePackageAsync(id);
    return r is null ? Results.NotFound(new { id, found = false }) : Results.Ok(r);
}).RequireAuthorization();

app.MapPut("/api/service-packages/{id:long}", async (long id, UpdateServicePackageDto dto, IBookingService svc) =>
{
    try
    {
        var r = await svc.UpdateServicePackageAsync(id, dto);
        return r is null ? Results.NotFound(new { id, error = "Không thấy gói dịch vụ." }) : Results.Ok(r);
    }
    catch (InvalidOperationException ex) { return Results.Conflict(new { id, error = ex.Message }); }
}).RequireAuthorization();

app.MapDelete("/api/service-packages/{id:long}", async (long id, IBookingService svc) =>
{
    var r = await svc.DeleteServicePackageAsync(id);
    return r is null ? Results.NotFound(new { id, error = "Không thấy gói dịch vụ." }) : Results.Ok(r);
}).RequireAuthorization();

// ---- Chiến dịch marketing (Ser_CampaignMarketing): điều kiện áp dụng + phụ tùng khuyến mãi ----
app.MapPost("/api/campaigns", async (CreateCampaignDto dto, IBookingService svc) =>
{
    if (string.IsNullOrWhiteSpace(dto.CamMarketingName)) return Results.BadRequest(new { error = "Cần CamMarketingName." });
    try { return Results.Ok(await svc.CreateCampaignAsync(dto)); }
    catch (InvalidOperationException ex) { return Results.Conflict(new { error = ex.Message }); }
}).RequireAuthorization();

app.MapGet("/api/campaigns", async (IBookingService svc, string? keyword, string? status, bool? active) =>
    Results.Ok(await svc.ListCampaignsAsync(keyword, status, active))).RequireAuthorization();

app.MapGet("/api/campaigns/{camMarketingNo}", async (string camMarketingNo, IBookingService svc) =>
{
    var r = await svc.GetCampaignAsync(camMarketingNo);
    return r is null ? Results.NotFound(new { camMarketingNo, found = false }) : Results.Ok(r);
}).RequireAuthorization();

app.MapDelete("/api/campaigns/{camMarketingNo}", async (string camMarketingNo, IBookingService svc) =>
{
    var r = await svc.DeleteCampaignAsync(camMarketingNo);
    return r is null ? Results.NotFound(new { camMarketingNo, error = "Không thấy chiến dịch." }) : Results.Ok(r);
}).RequireAuthorization();

// Ser_CampaignMarketing_GetForRoPartItem: lọc chiến dịch đang hiệu lực áp dụng cho xe/RO (kèm phụ tùng khuyến mãi).
app.MapGet("/api/campaigns/match", async (IBookingService svc, string? carIds, string? roIds, string? effDate) =>
    Results.Ok(await svc.MatchCampaignsAsync(new MatchCampaignsDto(carIds, roIds, effDate)))).RequireAuthorization();

// ---- Thiết lập bảo dưỡng định kỳ (Ser_MST_ROMaintanceSetting): mốc Km → số lần bảo dưỡng thỏa mãn CSBH ----
// Ser_MST_ROMaintanceSetting_Get: danh sách thiết lập (lọc khoảng Km + cờ hiệu lực + phân trang).
app.MapGet("/api/maintenance-settings", async (IBookingService svc, int? minKm, int? maxKm, bool? active, int? recordStart, int? recordCount) =>
    Results.Ok(await svc.ListMaintenanceSettingsAsync(minKm, maxKm, active, recordStart, recordCount))).RequireAuthorization();

// Gợi ý mốc bảo dưỡng kế tiếp theo số Km hiện tại của xe.
app.MapGet("/api/maintenance-settings/suggest", async (IBookingService svc, int km) =>
{
    var r = await svc.SuggestMaintenanceForKmAsync(km);
    return r is null ? Results.NotFound(new { km, error = "Không có mốc bảo dưỡng nào phù hợp." }) : Results.Ok(r);
}).RequireAuthorization();

app.MapGet("/api/maintenance-settings/{romsId}", async (string romsId, IBookingService svc) =>
{
    var r = await svc.GetMaintenanceSettingAsync(romsId);
    return r is null ? Results.NotFound(new { romsId, found = false }) : Results.Ok(r);
}).RequireAuthorization();

// Ser_MST_ROMaintanceSetting_Save: tạo/cập nhật thiết lập bảo dưỡng (validate Km dương + Maintances >= 0 + Km không trùng).
app.MapPost("/api/maintenance-settings", async (SaveMaintenanceSettingDto dto, IBookingService svc) =>
{
    try { return Results.Ok(await svc.SaveMaintenanceSettingAsync(dto)); }
    catch (InvalidOperationException ex) { return Results.Conflict(new { error = ex.Message }); }
}).RequireAuthorization();

app.MapDelete("/api/maintenance-settings/{romsId}", async (string romsId, IBookingService svc) =>
{
    var r = await svc.DeleteMaintenanceSettingAsync(romsId);
    return r is null ? Results.NotFound(new { romsId, error = "Không thấy thiết lập bảo dưỡng." }) : Results.Ok(r);
}).RequireAuthorization();

app.MapPost("/api/orgs/register", async (RegisterOrgDto dto, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(dto.Name)) return Results.BadRequest(new { error = "Cần Name." });
    var org = new Org { Name = dto.Name.Trim(), ApiKey = "bok_" + Guid.NewGuid().ToString("N") };
    db.Orgs.Add(org); await db.SaveChangesAsync();
    return Results.Ok(new { orgId = org.Id, apiKey = org.ApiKey });
});

// Import kỹ thuật viên thật từ Ser_Engineer (dedupe theo Code)
app.MapPost("/api/import/engineers", async (List<ImportEngineerDto> rows, AppDbContext db, ITenantContext tc) =>
{
    if (rows == null || rows.Count == 0) return Results.BadRequest(new { error = "Không có dữ liệu." });
    int added = 0, skipped = 0;
    var orgId = tc.OrgId;
    var existCodes = db.Engineers.Where(e => e.OrgId == orgId).Select(e => e.Code).ToHashSet();
    foreach (var row in rows)
    {
        if (string.IsNullOrWhiteSpace(row.Code)) { skipped++; continue; }
        if (existCodes.Contains(row.Code.Trim())) { skipped++; continue; }
        db.Engineers.Add(new Engineer { OrgId = orgId, Code = row.Code.Trim(), Name = row.Name?.Trim() ?? row.Code.Trim(), Skill = row.Skill, DealerCode = row.DealerCode ?? "", Active = row.Active });
        existCodes.Add(row.Code.Trim()); added++;
    }
    await db.SaveChangesAsync();
    return Results.Ok(new { added, skipped, total = added + skipped });
});

// Import lịch hẹn thật từ Ser_App/Ser_RO (dedupe theo Code)
app.MapPost("/api/import/appointments", async (List<ImportApptDto> rows, AppDbContext db, ITenantContext tc) =>
{
    if (rows == null || rows.Count == 0) return Results.BadRequest(new { error = "Không có dữ liệu." });
    int added = 0, skipped = 0;
    var orgId = tc.OrgId;
    var existCodes = db.Appointments.Where(a => a.OrgId == orgId).Select(a => a.Code).ToHashSet();
    foreach (var row in rows)
    {
        if (string.IsNullOrWhiteSpace(row.Code)) { skipped++; continue; }
        if (existCodes.Contains(row.Code.Trim())) { skipped++; continue; }
        db.Appointments.Add(new Appointment
        {
            OrgId = orgId, Code = row.Code.Trim(),
            CustomerName = row.CustomerName?.Trim() ?? "Khách hàng",
            Phone = row.Phone ?? "", Vin = row.Vin, Plate = row.Plate,
            ServiceType = row.ServiceType ?? "Bảo dưỡng",
            PreferredAt = row.PreferredAt ?? DateTime.Now,
            DealerCode = row.DealerCode ?? "", Engineer = row.Engineer,
            Status = (ApptStatus)Math.Clamp(row.Status, 0, 5), Note = row.Note, RoNo = row.RoNo
        });
        existCodes.Add(row.Code.Trim()); added++;
    }
    await db.SaveChangesAsync();
    return Results.Ok(new { added, skipped, total = added + skipped });
});

app.Run();

record RegisterOrgDto(string Name);
record ImportEngineerDto(string? Code, string? Name, string? Skill, string? DealerCode, bool Active);
record ImportApptDto(string? Code, string? CustomerName, string? Phone, string? Vin, string? Plate, string? ServiceType, DateTime? PreferredAt, string? DealerCode, string? Engineer, int Status, string? Note, string? RoNo);
record LinkRepairOrderDto(string AppCode);
