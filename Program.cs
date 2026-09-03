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
    return Results.Ok(await svc.BookAsync(dto));
});

app.MapGet("/api/book/{code}", async (string code, IBookingService svc) =>
{
    var r = await svc.StatusAsync(code);
    return r is null ? Results.NotFound(new { code, found = false }) : Results.Ok(r);
});

// ---- Nội bộ (SSO): quản lý lịch hẹn ----
app.MapGet("/api/appointments", async (IBookingService svc, string? status, string? dealer, string? date) =>
    Results.Ok(await svc.ListAsync(status, dealer, date))).RequireAuthorization();

app.MapPost("/api/appointments/{code}/confirm", async (string code, ConfirmDto dto, IBookingService svc) =>
{
    var r = await svc.ConfirmAsync(code, dto.Engineer);
    return r is null ? Results.NotFound(new { code, error = "Không thấy lịch chờ xác nhận." }) : Results.Ok(r);
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
    var r = await svc.CancelAsync(code, noShow ?? false);
    return r is null ? Results.NotFound(new { code, error = "Không hủy được (đã xong/đã hủy)." }) : Results.Ok(r);
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
