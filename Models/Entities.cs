namespace MiniBooking.Models;

public sealed class Org
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>Vòng đời lịch hẹn dịch vụ (Ser_App): khách đặt → xác nhận → check-in (tạo RO) → hoàn tất.</summary>
public enum ApptStatus { Requested = 0, Confirmed = 1, CheckedIn = 2, Done = 3, Cancelled = 4, NoShow = 5 }

/// <summary>Kỹ thuật viên (Ser_Engineer): roster + kỹ năng để phân lịch + đo tải.</summary>
public sealed class Engineer
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Skill { get; set; }         // Máy gầm/Điện/Đồng sơn...
    public string DealerCode { get; set; } = "";
    public bool Active { get; set; } = true;
}

/// <summary>Nhắc chăm sóc KH dịch vụ (Ser_CustomerCare): nhắc bảo dưỡng/sinh nhật/bảo hiểm → liên hệ → đặt lịch.</summary>
public sealed class CareReminder
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string CustomerName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? Vin { get; set; }
    public string? Plate { get; set; }
    public string CareType { get; set; } = "Maintenance";  // Maintenance/Birthday/Insurance/WarrantyExpiry
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "Pending";         // Pending → Contacted → Booked/Closed
    public string? Note { get; set; }
    public string? BookingCode { get; set; }                // lịch hẹn sinh ra (nếu chuyển)
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ContactedAt { get; set; }
}

/// <summary>Lịch hẹn dịch vụ (chuyển đổi Ser_App): 1 khách/1 xe/1 khung giờ tại 1 xưởng.</summary>
public sealed class Appointment
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? Vin { get; set; }
    public string? Plate { get; set; }
    public string ServiceType { get; set; } = "Bảo dưỡng";   // Bảo dưỡng/Sửa chữa/Bảo hành/Đồng sơn
    public DateTime PreferredAt { get; set; }
    public string DealerCode { get; set; } = "";
    public string? Engineer { get; set; }
    public ApptStatus Status { get; set; } = ApptStatus.Requested;
    public string? Note { get; set; }
    public string? RoNo { get; set; }             // số RO sinh khi check-in
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? CheckedInAt { get; set; }
    public DateTime? DoneAt { get; set; }
}
