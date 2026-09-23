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

// ---- Nội bộ (SSO): quản lý lịch hẹn ----
app.MapGet("/api/appointments", async (IBookingService svc, string? status, string? dealer, string? date) =>
    Results.Ok(await svc.ListAsync(status, dealer, date))).RequireAuthorization();

// Ser_App_GetStatusList01DL: tìm kiếm nâng cao lịch hẹn (đa giá trị '|', mẫu biển số, tên KH, loại cuộc hẹn, timeline, phân trang).
app.MapGet("/api/appointments/search", async (IBookingService svc, string? dealerCodes, string? statuses, string? platePattern,
    string? customerName, string? creator, string? appTypeCodes, string? dateFrom, string? dateTimeline, int? recordStart, int? recordCount) =>
    Results.Ok(await svc.SearchAppointmentsAsync(new SearchAppointmentsDto(
        dealerCodes, statuses, platePattern, customerName, creator, appTypeCodes, dateFrom, dateTimeline, recordStart, recordCount)))
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
