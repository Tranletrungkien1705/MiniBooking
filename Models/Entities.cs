namespace MiniBooking.Models;

public sealed class Org
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>Vòng đời lịch hẹn dịch vụ (Ser_App): khách đặt → liên hệ xác nhận → xác nhận → check-in (tạo RO) → hoàn tất.
/// Contacted = AppStatus=5 'Đã liên hệ &amp; Chưa xác nhận' (Ser_CustomerCare72h): tổng đài gọi xác nhận lịch trước giờ hẹn.</summary>
public enum ApptStatus { Requested = 0, Confirmed = 1, CheckedIn = 2, Done = 3, Cancelled = 4, NoShow = 5, Contacted = 6 }

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

/// <summary>Khoang sửa chữa (chuyển đổi Ser_Cavity): cầu nâng/khoang tiếp nhận theo xưởng, có sức chứa theo khung giờ.</summary>
public sealed class ServiceBay
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";          // CavityNo
    public string Name { get; set; } = "";          // CavityName
    public string BayType { get; set; } = "General"; // CavityType: General/Maintain/RO/Copper/Parking
    public int CapacityPerSlot { get; set; } = 1;    // số xe tối đa mỗi khung giờ
    public string DealerCode { get; set; } = "";
    public bool Active { get; set; } = true;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>Loại cuộc hẹn (chuyển đổi Mst_Ser_AppType): master loại lịch hẹn để phân loại + thống kê.</summary>
public sealed class AppType
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";        // AppTypeCode
    public string Name { get; set; } = "";        // AppTypeName
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>Loại khoang sửa chữa (chuyển đổi Mst_Compartment / Ser_Cavity.CavityType):
/// BDN bảo dưỡng nhanh · SCC sửa chữa chung (RO) · KD đồng · KS sơn (BP) · BS buồng sơn · KTN đỗ xe · KHAC khác.</summary>
public sealed class CavityType
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string Code { get; set; } = "";        // CompartmentCode / CavityType
    public string Name { get; set; } = "";
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>Ràng buộc khoang theo loại dịch vụ (Ser_Cavity.CavityType ↔ Ser_App.ServiceType):
/// mỗi loại dịch vụ chỉ được xếp vào các loại khoang tương thích.</summary>
public static class CavityRules
{
    // Mã loại khoang chuẩn theo 2023.H.CarServices (Ser_Cavity.CavityType).
    public const string Maintain = "BDN";   // Bảo dưỡng nhanh
    public const string Repair   = "SCC";   // Sửa chữa chung (RO)
    public const string Copper   = "KD";    // Đồng
    public const string Paint    = "KS";    // Sơn (BP)
    public const string PaintBooth = "BS";  // Buồng sơn
    public const string Parking  = "KTN";   // Đỗ xe / giao xe
    public const string Other    = "KHAC";  // Khác (kiểm tra cuối, thử phanh)

    /// <summary>Loại khoang được phép cho một loại dịch vụ (ServiceType).</summary>
    public static string[] AllowedFor(string? serviceType)
    {
        var s = (serviceType ?? "").Trim().ToLowerInvariant();
        if (s.Contains("bảo dưỡng") || s.Contains("bao duong") || s.Contains("maintain"))
            return new[] { Maintain, Other };
        if (s.Contains("đồng") || s.Contains("dong") || s.Contains("copper"))
            return new[] { Copper, Paint, PaintBooth, Other };
        if (s.Contains("sơn") || s.Contains("son") || s.Contains("paint") || s.Contains("đồng sơn") || s.Contains("dong son"))
            return new[] { Paint, PaintBooth, Copper, Other };
        if (s.Contains("sửa chữa") || s.Contains("sua chua") || s.Contains("repair") || s.Contains("ro"))
            return new[] { Repair, Other };
        if (s.Contains("bảo hành") || s.Contains("bao hanh") || s.Contains("warranty"))
            return new[] { Repair, Other };
        // Mặc định: cho phép mọi loại khoang (không chặn khi chưa phân loại rõ).
        return Array.Empty<string>();
    }

    /// <summary>True nếu khoang loại <paramref name="bayType"/> phù hợp với loại dịch vụ.</summary>
    public static bool IsCompatible(string? serviceType, string? bayType)
    {
        var allowed = AllowedFor(serviceType);
        if (allowed.Length == 0) return true;                 // chưa rõ loại → không chặn
        var bt = (bayType ?? "").Trim().ToUpperInvariant();
        if (bt.Length == 0) return true;                      // khoang chưa gán loại → không chặn
        return allowed.Contains(bt);
    }
}

/// <summary>Dịch vụ đăng ký kèm lịch hẹn (chuyển đổi Ser_AppServiceItems): danh sách công việc
/// khách yêu cầu khi đặt lịch, kèm giờ công chuẩn (StdManHour) để ước lượng thời lượng.</summary>
public sealed class AppServiceItem
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string AppCode { get; set; } = "";     // mã lịch hẹn (Ser_App.Code)
    public string SerCode { get; set; } = "";     // mã công việc (Ser_Mst_Service.SerCode)
    public string SerName { get; set; } = "";     // tên công việc
    public decimal StdManHour { get; set; }        // giờ công chuẩn
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>Phụ tùng đăng ký kèm lịch hẹn (chuyển đổi Ser_AppPartItems): danh sách phụ tùng
/// khách dự kiến cần khi đặt lịch, kèm số lượng + tồn kho để chuẩn bị trước.</summary>
public sealed class AppPartItem
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string AppCode { get; set; } = "";     // mã lịch hẹn (Ser_App.Code)
    public string PartCode { get; set; } = "";    // mã phụ tùng (Mst_Part.PartCode)
    public string PartName { get; set; } = "";    // tên phụ tùng (VieName)
    public string Unit { get; set; } = "";        // đơn vị tính
    public decimal Quantity { get; set; }          // số lượng cần
    public decimal InventoryQuantity { get; set; } // tồn kho tại thời điểm đăng ký
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>Lệnh sửa chữa / báo giá (chuyển đổi Ser_RO): phiếu tiếp nhận xe vào xưởng.
/// Một RO có thể sinh ra 1 cuộc hẹn (Ser_RO.AppId ↔ Ser_App.ROID) — dùng để nối lịch hẹn với lệnh sửa chữa.</summary>
public sealed class RepairOrder
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string RoId { get; set; } = "";        // ROID (mã lệnh sửa chữa)
    public string RoNo { get; set; } = "";        // RONo (số báo giá)
    public string DealerCode { get; set; } = "";
    public string CusName { get; set; } = "";     // CusName
    public string? CusTel { get; set; }            // CusTel
    public string? PlateNo { get; set; }           // PlateNo
    public string? FrameNo { get; set; }           // FrameNo
    public string? CusRequest { get; set; }        // Yêu cầu khách hàng
    public string Status { get; set; } = "Open";   // trạng thái báo giá
    public string? AppCode { get; set; }           // AppId — cuộc hẹn đã gắn (Ser_RO.AppId)
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? LinkedAt { get; set; }        // thời điểm gắn cuộc hẹn
}

/// <summary>Chăm sóc KH sau dịch vụ 72h (chuyển đổi Ser_CustomerCare72h): khảo sát hài lòng sau khi giao xe.
/// Vòng đời: PEND (chưa liên hệ) → CINFB (đã liên hệ, chưa phản hồi) / CIFB (đã liên hệ, đã phản hồi) / REJ (bỏ qua).
/// Gắn với 1 lệnh sửa chữa (ROID) + ngày giao xe (FinishedDate).</summary>
public sealed class PostServiceCare
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string CusCareId { get; set; } = "";      // CusCareID (mã phiếu chăm sóc)
    public string? RoId { get; set; }                 // ROID — lệnh sửa chữa nguồn
    public string? RoNo { get; set; }                 // RONo
    public string CustomerName { get; set; } = "";   // CusName
    public string? Phone { get; set; }                // Mobile/Tel
    public string? Plate { get; set; }                // PlateNo
    public string? FrameNo { get; set; }              // FrameNo
    public string DealerCode { get; set; } = "";
    public DateTime? FinishedDate { get; set; }       // FinishedDate — ngày giao xe (mốc tính 72h)
    public string Status { get; set; } = "PEND";     // PEND/CINFB/CIFB/REJ
    public DateTime? ContactDate { get; set; }        // ContactDate — ngày liên hệ
    // Trả lời khảo sát (Ser_CustomerCare72h):
    public string? FyourCSSH { get; set; }            // Đánh giá chăm sóc khách hàng
    public string? WFBasicNeeds { get; set; }         // Nhu cầu cơ bản được đáp ứng
    public string? YourCarProblem { get; set; }       // Vấn đề của xe sau dịch vụ
    public string? YourRIWN { get; set; }             // Sẽ giới thiệu (Recommend/Introduce/Would)
    public string? YourSatisfyQSv { get; set; }       // Hài lòng chất lượng dịch vụ
    public string? YourHopeOfOur { get; set; }        // Mong muốn của khách
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>Phiếu tiếp nhận xe (chuyển đổi Ser_ReceptionF): lập khi khách đến xưởng, ghi nhận tình trạng xe lúc giao.
/// Vòng đời: P (Tiếp nhận) → A (Giao xe). Chỉ xóa được khi chưa phát sinh lệnh sửa chữa (Ser_RO.ReceptionFNo).</summary>
public sealed class ReceptionForm
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string ReceptionFNo { get; set; } = "";   // ReceptionFNo (mã phiếu tiếp nhận)
    public string DealerCode { get; set; } = "";
    public string CusName { get; set; } = "";        // CusName
    public string? Phone { get; set; }                // Tel/Mobile
    public string? Plate { get; set; }                // PlateNo
    public string? FrameNo { get; set; }              // FrameNo
    public string ReceptionType { get; set; } = "Service"; // ReceptionType: Service/Repair/Warranty...
    public string Status { get; set; } = "P";        // ReceptionFStatus: P=Tiếp nhận, A=Giao xe
    public string? CreatedBy { get; set; }            // CreatedBy (người lập phiếu)
    public DateTime CreatedDateTime { get; set; } = DateTime.Now;  // CreatedDateTime
    public DateTime? DeliveryDateTime { get; set; }   // DeliveryDateTime (thời điểm giao xe)
    public string? AppCode { get; set; }              // lịch hẹn nguồn (Ser_App.Code) nếu tiếp nhận từ lịch hẹn
    public string? RoNo { get; set; }                 // số RO phát sinh (Ser_RO.RONo) — chặn xóa khi đã có
    public string? Note { get; set; }
}

/// <summary>Các công đoạn sửa chữa (Ser_AssignmentWork_WorkType): mỗi công đoạn có khoang + giờ kế hoạch/thực tế riêng.</summary>
public static class WorkStages
{
    public const string SCC   = "SCC";   // Sửa chữa chung
    public const string SCD   = "SCD";   // Sửa chữa đồng
    public const string SCN   = "SCN";   // Sửa chữa nền
    public const string SCS   = "SCS";   // Sửa chữa sơn
    public const string SCDB  = "SCDB";  // Sửa chữa đánh bóng
    public const string SCLR  = "SCLR";  // Sửa chữa lắp ráp
    public const string SCKSC = "SCKSC"; // Sửa chữa KSC - Vệ sinh

    /// <summary>Danh sách mã công đoạn hợp lệ (theo thứ tự Ser_AssignmentWork_WorkType).</summary>
    public static readonly string[] All = { SCC, SCD, SCN, SCS, SCDB, SCLR, SCKSC };

    /// <summary>Tên hiển thị tiếng Việt của công đoạn.</summary>
    public static string Text(string? code) => (code ?? "").Trim().ToUpperInvariant() switch
    {
        SCC   => "Sửa chữa chung",
        SCD   => "Sửa chữa đồng",
        SCN   => "Sửa chữa nền",
        SCS   => "Sửa chữa sơn",
        SCDB  => "Sửa chữa đánh bóng",
        SCLR  => "Sửa chữa lắp ráp",
        SCKSC => "Sửa chữa KSC - Vệ sinh",
        _     => code ?? ""
    };
}

/// <summary>Phân công công việc sửa chữa (chuyển đổi Ser_AssignmentWork): gắn 1 lệnh sửa chữa (ROID)
/// với kế hoạch/thực tế theo từng công đoạn (SCC/SCD/SCN/SCS/SCDB/SCLR/SCKSC), mỗi công đoạn có khoang riêng.
/// Chặn trùng khung giờ kế hoạch trên cùng khoang (MyCheck_SerAssignmentWork_PlanDateTime_Cavity).</summary>
public sealed class WorkAssignment
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string RoId { get; set; } = "";        // ROID — lệnh sửa chữa được phân công
    public string? RoNo { get; set; }              // RONo (tiện hiển thị)
    public string DealerCode { get; set; } = "";
    public string? WorkTypeStart { get; set; }     // WorkTypeStart — công đoạn bắt đầu
    public string? WorkTypeFinish { get; set; }    // WorkTypeFinish — công đoạn kết thúc
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public List<WorkAssignmentStage> Stages { get; set; } = new();
}

/// <summary>Kế hoạch/thực tế 1 công đoạn trong phân công công việc (chuyển đổi Ser_AssignmentWork theo từng cột SCC/SCD/...).</summary>
public sealed class WorkAssignmentStage
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public long AssignmentId { get; set; }         // FK → WorkAssignment
    public string WorkType { get; set; } = "";     // SCC/SCD/SCN/SCS/SCDB/SCLR/SCKSC
    public string? CavityCode { get; set; }        // khoang (Ser_Cavity.CavityNo)
    public DateTime? PlanStart { get; set; }       // PlanStartDTime
    public DateTime? PlanFinish { get; set; }      // PlanFinishDTime
    public DateTime? ActualStart { get; set; }     // ActualStartDTime
    public DateTime? ActualFinish { get; set; }    // ActualFinishDTime
}

/// <summary>Kỹ thuật viên được phân công cho 1 lệnh sửa chữa theo công đoạn (chuyển đổi Ser_AssignmentWorkEngineer).</summary>
public sealed class WorkAssignmentEngineer
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public long AssignmentId { get; set; }         // FK → WorkAssignment
    public string RoId { get; set; } = "";        // ROID
    public string EngineerCode { get; set; } = ""; // EngineerID/EngineerNo
    public string WorkType { get; set; } = "";     // SCC (chung) hoặc SCDS (đồng sơn)
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
    public string? BayCode { get; set; }          // khoang sửa chữa (Ser_Cavity) — gán khi xác nhận
    public string? AppTypeCode { get; set; }      // loại cuộc hẹn (Mst_Ser_AppType)
    public DateTime? SlotFrom { get; set; }       // khung giờ bắt đầu (AppDateTimeFrom)
    public DateTime? SlotTo { get; set; }         // khung giờ kết thúc (AppDateTime)
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? CheckedInAt { get; set; }
    public DateTime? DoneAt { get; set; }
    public DateTime? ContactedAt { get; set; }    // thời điểm gọi xác nhận (Ser_CustomerCare72h.ConatctDate)
    public string? ContactNote { get; set; }      // ghi chú cuộc gọi (Ser_CustomerCare72h.Note)
    public string? ContactResult { get; set; }    // kết quả: Confirmed/NoAnswer/Rejected (CIFB/CINFB/REJ)
    public string? RoId { get; set; }             // lệnh sửa chữa nguồn (Ser_App.ROID ↔ Ser_RO.ROID)
}
