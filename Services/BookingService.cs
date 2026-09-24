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
// Ser_CustomerCareBth: nhắc chăm sóc sinh nhật KH (tạo/cập nhật dòng nhắc + cập nhật trạng thái liên hệ).
public record CreateBirthdayCareDto(string? CareBthId, string? DealerCode, string CusId, string CustomerName, string? Phone, string? Email, string? Plate, string? FrameNo, string? TradeMarkCode, string? ModelName, DateTime? DateBth, string? Remark);
public record UpdateBirthdayCareDto(string? Status, string? ContactDate, string? Remark, DateTime? DateBth);
public record AddEngineerDto(string Code, string Name, string? Skill, string? DealerCode);
public record AddBayDto(string Code, string Name, string? BayType, int? CapacityPerSlot, string? DealerCode, string? Note, string? StartUseDate = null, string? FinishUseDate = null, string? Status = null);
public record AddAppTypeDto(string Code, string Name);
public record AddCavityTypeDto(string Code, string Name);
// Ser_GroupRepair: tổ kỹ thuật (Quản lý tổ kỹ thuật) — tạo/cập nhật master nhóm KTV theo xưởng.
public record SaveRepairGroupDto(string? GroupRNo, string? GroupRName, string? DealerCode, string? Note, bool? IsActive, string? ChangedBy);
// Mst_Calendar_ResetYear: khởi tạo lịch làm việc cả năm theo StatusValue từng thứ (0 = làm việc).
public record ResetCalendarYearDto(string? CalendarType, int Year, int? Monday, int? Tuesday, int? Wednesday, int? Thursday, int? Friday, int? Saturday, int? Sunday);
// Mst_Calendar_UpdateStatusValue: đổi trạng thái làm việc/nghỉ của 1 ngày cụ thể.
public record UpdateCalendarDayDto(string? CalendarType, string Date, int StatusValue);
public record AddServiceItemDto(string SerCode, string? SerName, decimal? StdManHour, string? Note);
public record AddPartItemDto(string PartCode, string? PartName, string? Unit, decimal? Quantity, decimal? InventoryQuantity, string? Note);
public record AddRepairOrderDto(string RoId, string? RoNo, string? DealerCode, string? CusName, string? CusTel, string? PlateNo, string? FrameNo, string? CusRequest, string? Status);
// Ser_ROServiceItems: dòng công việc trong lệnh sửa chữa (loại công việc + đối tượng thanh toán + giá/VAT + trạng thái xong).
public record AddRoServiceItemDto(string SerCode, string? SerName, string? ROType, string? ExpenseType, decimal? StdManHour, decimal? Factor, decimal? Price, decimal? VAT, bool? FlagAccrual, string? Note);
// Ser_RO_Update_ServiceItemsStatusRODL: cập nhật trạng thái hoàn thành của các dòng công việc trong RO.
public record RoServiceItemStatusDto(string SerCode, bool Status);
public record UpdateRoServiceItemsStatusDto(List<RoServiceItemStatusDto> Items, string? ChangedBy);
// Ser_RO_UpdateStatus: chuyển trạng thái lệnh sửa chữa theo máy trạng thái Ser_RO_Stage.
public record ChangeRoStatusDto(string ToStatus, string? Note, string? ChangedBy);
// SerROToRORejectStatusDL / SerROController.UpdateStatusToRejectRODL: hủy/từ chối lệnh sửa chữa
// (bắt buộc RejectDate + RejectNote; ghi lịch sử REJ; xóa phân công công việc của RO).
public record RejectRepairOrderDto(string RejectDate, string RejectNote, string? ChangedBy);
// Ser_RO_UpdatePlanedDeliveryDateDL: lưu ngày giao xe dự kiến của lệnh sửa chữa (kèm lý do).
public record UpdatePlannedDeliveryDateDto(string PlanedDeliveryDate, string? Remark, string? ChangedBy);
// Ser_RO_Sumary_DL: thống kê lệnh sửa chữa theo ngày (lọc đại lý '|', khoảng ngày CheckInDate, trạng thái '|').
public record RoSummaryDto(string? DealerCodes, string? FromDate, string? ToDate, string? Statuses);
// Ser_RO_Update_Maintance_DL: cập nhật thông tin nhắc bảo dưỡng kế tiếp của lệnh sửa chữa
// (Km hiện tại + ngày/mốc Km nhắc bảo dưỡng + công việc cần làm sớm + mã hội viên).
public record UpdateRoMaintenanceDto(int? Km, string? ReminderMaintanceDate, int? ReminderMaintanceKm, string? WorkDoneSoon, string? MemberNo, string? ChangedBy);
public record SlotQueryDto(string Date, string? BayCode, string? DealerCode);
public record CreatePostCareDto(string CusCareId, string? RoId, string? RoNo, string CustomerName, string? Phone, string? Plate, string? FrameNo, string? DealerCode, DateTime? FinishedDate, string? Note);
public record PostCareContactDto(string? ContactDate, string? FyourCSSH, string? WFBasicNeeds, string? YourCarProblem, string? YourRIWN, string? YourSatisfyQSv, string? YourHopeOfOur, string? Note);
// Ser_CustomerCare24h: phiếu chăm sóc sau dịch vụ 24h (khảo sát sớm, song song phiếu 72h).
public record CreatePostCare24hDto(string CusCareId, string? RoId, string? OrderId, string CustomerName, string? Phone, string? Plate, string? FrameNo, string? DealerCode, DateTime? FinishedDate24, string? Note);
public record PostCare24hContactDto(string? ContactDate24, string? FyourCSSH24, string? WFBasicNeeds24, string? YourCarProblem24, string? YourRIWN24, string? YourSatisfyQSv24, string? YourHopeOfOur24, string? Note);
public record CreateReceptionFormDto(string? ReceptionFNo, string? DealerCode, string CustomerName, string? Phone, string? Plate, string? FrameNo, string? ReceptionType, string? AppCode, string? Note);
public record DeliverReceptionFormDto(string? RoNo, string? Note);
// Ser_App_GetStatusList01DL: bộ lọc nâng cao danh sách lịch hẹn (đa giá trị '|', mẫu biển số, khoảng thời gian, timeline).
public record SearchAppointmentsDto(string? DealerCodes, string? Statuses, string? PlatePattern, string? CustomerName, string? Creator, string? AppTypeCodes, string? DateFrom, string? DateTimeline, int? RecordStart, int? RecordCount);
// Ser_App_GetNewDL: tìm lịch hẹn theo bộ lọc "GetNew" (AppId/AppNo/CreatedDate/Creator/AppStatus/AppDateTime/PlateNo/CusName/DealerCode)
// + phân trang + tùy chọn mở rộng chi tiết (kèm dịch vụ & phụ tùng ngay trong kết quả).
public record GetNewAppointmentsDto(string? AppIds, string? DealerCodes, string? PlateNos, string? AppNos, string? CustomerNames,
    string? CreatedDates, string? AppDateTimes, string? Statuses, string? Creators,
    bool? IncludeApp, bool? IncludeServiceItems, bool? IncludePartItems, int? RecordStart, int? RecordCount);
// Ser_App_GetForCavityDL: tìm lịch hẹn để xếp khoang — lọc theo biển số (chứa), 1 ngày cụ thể,
// và 4 cờ loại cuộc hẹn (BDDK bảo dưỡng định kỳ / SCC sửa chữa chung / SCDS sửa chữa đồng sơn / SCK sửa chữa khác).
public record GetForCavityDto(string? PlateNo, string? DateTimeLine, bool? FlagBDDK, bool? FlagSCC, bool? FlagSCDS, bool? FlagSCK);
// Ser_App_GetStatusList01WHDL (SerAppSearchWHDL): tìm lịch hẹn theo KHOẢNG NGÀY hẹn (AppDateTimeFrom..AppDateTimeTo)
// + 5 cờ trạng thái (Mới tạo/Xác nhận/Đã liên hệ/Tiếp nhận/Hủy) + biển số/tên KH/người tạo (chứa) + phân trang theo trang.
public record SearchAppointmentsWhDto(string? DealerCode, string? AppDateTimeFrom, string? AppDateTimeTo,
    string? PlateNo, string? CustomerName, string? Creator, string? AppTypeCodes,
    bool? FlagMoiTao, bool? FlagXacNhan, bool? FlagDaLienHe, bool? FlagTiepNhan, bool? FlagHuy,
    int? PageIndex, int? PageSize);
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
// Ser_ServicePackage: gói dịch vụ (nhóm sẵn công việc + phụ tùng theo 1 giá gói).
public record PackageServiceItemDto(string SerCode, string? SerName, decimal? Factor, decimal? ActManHour, decimal? VAT, decimal? Price, string? ExpenseType, string? ROType, string? Note);
public record PackagePartItemDto(string PartCode, string? PartName, string? Unit, decimal? Factor, decimal? Quantity, decimal? VAT, decimal? Price, string? ExpenseType, string? Note);
public record CreateServicePackageDto(string? PackageNo, string? PackageName, string? DealerCode, string? TakingTime, string? Description,
    string? Creator, bool? IsPublicFlag, bool? IsUserBasePrice, List<PackageServiceItemDto>? ServiceItems, List<PackagePartItemDto>? PartItems);
public record UpdateServicePackageDto(string? PackageName, string? DealerCode, string? TakingTime, string? Description,
    bool? IsPublicFlag, bool? IsUserBasePrice, List<PackageServiceItemDto>? ServiceItems, List<PackagePartItemDto>? PartItems);
// Ser_CampaignMarketing: chiến dịch marketing (điều kiện áp dụng + phụ tùng khuyến mãi).
public record CampaignConditionDto(string ConditionType, string Value);
public record CampaignPartDto(string PartCode, string? PartName, string? Unit, decimal? Quantity, decimal? Price, string? Note);
public record CreateCampaignDto(string? CamMarketingNo, string CamMarketingName, string? Description, string? CamMarketingStatus,
    DateTime? EffDateStart, DateTime? EffDateEnd, DateTime? WarrantyDateStart, DateTime? WarrantyDateEnd,
    bool? ConditionPlateNo, bool? ConditionDealer, bool? ConditionVIN, bool? ConditionFullVIN,
    List<CampaignConditionDto>? Conditions, List<CampaignPartDto>? Parts);
// Ser_CampaignMarketing_GetForRoPartItem: lọc chiến dịch áp dụng cho xe/RO (CarID/ROID + ngày hiệu lực).
public record MatchCampaignsDto(string? CarIds, string? RoIds, string? EffDate);
// Ser_MST_ROMaintanceSetting: thiết lập bảo dưỡng định kỳ (mốc Km → số lần bảo dưỡng thỏa mãn CSBH).
public record SaveMaintenanceSettingDto(string? RomsId, int Km, int Maintances, bool? FlagActive, string? LogLUBy);
// Ser_App_UpdateStatusDL: đổi trạng thái lịch hẹn theo máy trạng thái AppStatus (1..5) + ghi lịch sử.
public record ChangeApptStatusDto(string ToStatus, string? Note, string? ChangedBy);

public interface IBookingService
{
    Task<object> BookAsync(BookDto dto);        // công khai (khách)
    Task<object?> StatusAsync(string code);     // công khai
    Task<object?> GetAppointmentDetailAsync(string code);  // chi tiết lịch hẹn (Ser_App_GetDL/GetByAppIdDL)
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
    Task<object> CreateBirthdayCareAsync(CreateBirthdayCareDto dto);   // tạo/cập nhật nhắc sinh nhật (Ser_CustomerCareBth)
    Task<object> ListBirthdayCaresAsync(string? status, string? dealer, string? customerName, string? plate, string? frameNo, string? dateBth, int? recordStart, int? recordCount);  // tìm nhắc sinh nhật (Ser_CustomerCareBth_Get_DL)
    Task<object?> GetBirthdayCareAsync(string careBthId);              // chi tiết 1 nhắc sinh nhật
    Task<object?> UpdateBirthdayCareAsync(string careBthId, UpdateBirthdayCareDto dto);  // cập nhật trạng thái/liên hệ (Ser_CustomerCareBth_Update)
    Task<object> BirthdayCareStatsAsync();                             // thống kê nhắc sinh nhật
    Task<object> AddEngineerAsync(AddEngineerDto dto);
    Task<object> EngineerWorkloadAsync(string date, string? dealer);
    Task<object> AddBayAsync(AddBayDto dto);
    Task<object> ListBaysAsync(string? dealer, string? statusUse = null);
    Task<object> SlotAvailabilityAsync(string date, string? bayCode, string? dealer);
    Task<object> AddAppTypeAsync(AddAppTypeDto dto);
    Task<object> ListAppTypesAsync(bool? active);
    Task<object> AddCavityTypeAsync(AddCavityTypeDto dto);
    Task<object> ListCavityTypesAsync(bool? active);
    Task<object> SaveRepairGroupAsync(SaveRepairGroupDto dto);                       // tạo/cập nhật tổ kỹ thuật (Ser_GroupRepair_Create/Update)
    Task<object> ListRepairGroupsAsync(string? dealer, string? keyword, bool? active, int? recordStart, int? recordCount);  // tìm tổ kỹ thuật (Ser_GroupRepair_Get_DL)
    Task<object?> GetRepairGroupAsync(long id);                                      // chi tiết 1 tổ kỹ thuật
    Task<object?> DeleteRepairGroupAsync(long id);                                   // xóa tổ kỹ thuật (Ser_GroupRepair_Delete)
    Task<object> ResetCalendarYearAsync(ResetCalendarYearDto dto);            // khởi tạo lịch làm việc cả năm (Mst_Calendar_ResetYear)
    Task<object> ListCalendarDaysAsync(string? calendarType, int? year, string? from, string? to);  // danh sách ngày (Mst_Calendar_Get)
    Task<object?> UpdateCalendarDayAsync(UpdateCalendarDayDto dto);           // đổi trạng thái 1 ngày (Mst_Calendar_UpdateStatusValue)
    Task<object?> NextWorkingDayAsync(string from, int dayOffset);            // ngày làm việc thứ N kể từ mốc (Mst_Calendar_GetDateToCheck)
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
    Task<object?> ChangeRepairOrderStatusAsync(string roId, ChangeRoStatusDto dto);  // chuyển trạng thái RO (Ser_RO_UpdateStatus)
    Task<object?> RejectRepairOrderAsync(string roId, RejectRepairOrderDto dto);    // hủy/từ chối RO (SerROToRORejectStatusDL)
    Task<object?> GetRepairOrderStatusHistoryAsync(string roId);             // lịch sử đổi trạng thái RO (Ser_ROHistory)
    Task<object?> UpdatePlannedDeliveryDateAsync(string roId, UpdatePlannedDeliveryDateDto dto);  // lưu ngày giao xe dự kiến (Ser_RO_UpdatePlanedDeliveryDateDL)
    Task<object?> GetPlannedDeliveryDateHistoryAsync(string roId);           // lịch sử ngày giao xe dự kiến (Ser_Ro_PlanedDeliveryDate_His)
    Task<object> SummarizeRepairOrdersAsync(RoSummaryDto dto);               // thống kê lệnh sửa chữa theo ngày + doanh thu (Ser_RO_Sumary_DL)
    Task<object?> UpdateRoMaintenanceAsync(string roId, UpdateRoMaintenanceDto dto);  // cập nhật thông tin nhắc bảo dưỡng của RO (Ser_RO_Update_Maintance_DL)
    Task<object?> GetRepairOrderForAppointmentAsync(string roId);            // dữ liệu RO để tạo lịch hẹn (Ser_RO_GetForSerAppDL)
    Task<object?> AddRoServiceItemAsync(string roId, AddRoServiceItemDto dto);       // thêm dòng công việc vào RO (Ser_ROServiceItems)
    Task<object?> ListRoServiceItemsAsync(string roId);                              // danh sách công việc của RO + tổng tiền
    Task<object?> RemoveRoServiceItemAsync(string roId, long itemId);                // bỏ 1 dòng công việc khỏi RO
    Task<object?> UpdateRoServiceItemsStatusAsync(string roId, UpdateRoServiceItemsStatusDto dto); // cập nhật trạng thái xong + tự đổi ServiceStatus của RO
    Task<object> CreatePostCareAsync(CreatePostCareDto dto);                 // tạo phiếu chăm sóc sau dịch vụ 72h (Ser_CustomerCare72h)
    Task<object> ListPostCaresAsync(string? status, string? dealer, string? dueBefore);  // danh sách phiếu chăm sóc 72h
    Task<object?> GetPostCareAsync(string cusCareId);                        // chi tiết 1 phiếu chăm sóc 72h
    Task<object?> ContactPostCareAsync(string cusCareId, PostCareContactDto dto);  // ghi nhận liên hệ + trả lời khảo sát (CINFB/CIFB)
    Task<object?> RejectPostCareAsync(string cusCareId, string? note);       // bỏ qua không liên hệ (REJ)
    Task<object> PostCareStatsAsync();                                       // thống kê phiếu chăm sóc 72h
    Task<object> CreatePostCare24hAsync(CreatePostCare24hDto dto);           // tạo phiếu chăm sóc sau dịch vụ 24h (Ser_CustomerCare24h)
    Task<object> ListPostCares24hAsync(string? status, string? dealer, string? dueBefore, string? customerName, string? plate, string? frameNo);  // danh sách phiếu chăm sóc 24h
    Task<object?> GetPostCare24hAsync(string cusCareId);                     // chi tiết 1 phiếu chăm sóc 24h
    Task<object?> ContactPostCare24hAsync(string cusCareId, PostCare24hContactDto dto);  // ghi nhận liên hệ + trả lời khảo sát (CINFB/CIFB)
    Task<object?> RejectPostCare24hAsync(string cusCareId, string? note);    // bỏ qua không liên hệ (REJ)
    Task<object> PostCare24hStatsAsync();                                    // thống kê phiếu chăm sóc 24h
    Task<object> CreateReceptionFormAsync(CreateReceptionFormDto dto);       // lập phiếu tiếp nhận xe (Ser_ReceptionF)
    Task<object> ListReceptionFormsAsync(string? status, string? dealer, string? date);  // danh sách phiếu tiếp nhận
    Task<object?> GetReceptionFormAsync(string receptionFNo);                // chi tiết 1 phiếu tiếp nhận
    Task<object?> DeliverReceptionFormAsync(string receptionFNo, DeliverReceptionFormDto dto);  // giao xe (P → A)
    Task<object?> DeleteReceptionFormAsync(string receptionFNo);             // xóa phiếu (chặn khi đã có RO)
    Task<object> SearchAppointmentsAsync(SearchAppointmentsDto dto);         // tìm kiếm nâng cao lịch hẹn (Ser_App_GetStatusList01DL)
    Task<object> SearchAppointmentsWhAsync(SearchAppointmentsWhDto dto);     // tìm lịch hẹn theo khoảng ngày + cờ trạng thái (Ser_App_GetStatusList01WHDL)
    Task<object> GetNewAppointmentsAsync(GetNewAppointmentsDto dto);         // tìm lịch hẹn "GetNew" + mở rộng chi tiết (Ser_App_GetNewDL)
    Task<object> GetForCavityAsync(GetForCavityDto dto);                     // tìm lịch hẹn để xếp khoang (Ser_App_GetForCavityDL)
    Task<object?> UpdateAppointmentAsync(string code, UpdateAppointmentDto dto);  // sửa lịch hẹn (Ser_App_UpdateDL)
    Task<object> CreateWorkAssignmentAsync(CreateWorkAssignmentDto dto);          // phân công công việc sửa chữa (Ser_AssignmentWork_CreateDL)
    Task<object> ListWorkAssignmentsAsync(string? roId, string? dealer, string? date);  // danh sách phân công (Ser_AssignmentWork_Get_DL)
    Task<object?> GetWorkAssignmentAsync(string roId);                            // chi tiết phân công theo ROID
    Task<object?> UpdateWorkAssignmentAsync(string roId, UpdateWorkAssignmentDto dto);  // sửa phân công (Ser_AssignmentWork_UpdateDL)
    Task<object?> DeleteWorkAssignmentAsync(string roId);                         // xóa phân công (Ser_AssignmentWork_DeleteDL)
    Task<object> CreateServicePackageAsync(CreateServicePackageDto dto);          // tạo gói dịch vụ (Ser_ServicePackage_Create)
    Task<object> ListServicePackagesAsync(string? dealer, string? keyword, bool? isPublic);  // danh sách gói dịch vụ (Ser_ServicePackage_Get_DL)
    Task<object?> GetServicePackageAsync(long id);                                // chi tiết gói dịch vụ (kèm dịch vụ + phụ tùng)
    Task<object?> UpdateServicePackageAsync(long id, UpdateServicePackageDto dto); // sửa gói dịch vụ (Ser_ServicePackage_Update)
    Task<object?> DeleteServicePackageAsync(long id);                             // xóa gói dịch vụ (Ser_ServicePackage_Delete)
    Task<object> CreateCampaignAsync(CreateCampaignDto dto);                      // tạo/cập nhật chiến dịch marketing (Ser_CampaignMarketing)
    Task<object> ListCampaignsAsync(string? keyword, string? status, bool? active);  // danh sách chiến dịch (Ser_CampaignMarketing_SearchDL)
    Task<object?> GetCampaignAsync(string camMarketingNo);                        // chi tiết chiến dịch (kèm điều kiện + phụ tùng)
    Task<object?> DeleteCampaignAsync(string camMarketingNo);                     // xóa chiến dịch + điều kiện + phụ tùng
    Task<object> MatchCampaignsAsync(MatchCampaignsDto dto);                      // lọc chiến dịch áp dụng cho xe/RO (Ser_CampaignMarketing_GetForRoPartItem)
    Task<object> ListMaintenanceSettingsAsync(int? minKm, int? maxKm, bool? active, int? recordStart, int? recordCount);  // danh sách thiết lập bảo dưỡng (Ser_MST_ROMaintanceSetting_Get)
    Task<object?> GetMaintenanceSettingAsync(string romsId);                      // chi tiết 1 thiết lập bảo dưỡng theo ROMSID
    Task<object> SaveMaintenanceSettingAsync(SaveMaintenanceSettingDto dto);      // tạo/cập nhật thiết lập bảo dưỡng (Ser_MST_ROMaintanceSetting_Save)
    Task<object?> DeleteMaintenanceSettingAsync(string romsId);                   // xóa thiết lập bảo dưỡng
    Task<object?> SuggestMaintenanceForKmAsync(int km);                           // gợi ý mốc bảo dưỡng kế tiếp theo số Km hiện tại
    Task<object?> ChangeAppointmentStatusAsync(string code, ChangeApptStatusDto dto);  // đổi trạng thái lịch hẹn (Ser_App_UpdateStatusDL)
    Task<object?> GetAppointmentStatusHistoryAsync(string code);                  // lịch sử đổi trạng thái lịch hẹn
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

    // Ser_App_GetDL / SerAppController.GetByAppIdDL: chi tiết 1 lịch hẹn — header (khách/xe/khoang/KTV/RO)
    // + danh sách dịch vụ (Ser_AppServiceItems) + danh sách phụ tùng (Ser_AppPartItems) trong 1 lần gọi.
    public async Task<object?> GetAppointmentDetailAsync(string code)
    {
        var a = await Get(code);
        if (a is null) return null;

        var services = await db.AppServiceItems.Where(x => x.OrgId == Org && x.AppCode == a.Code)
            .OrderBy(x => x.SerCode)
            .Select(x => new { x.Id, x.SerCode, x.SerName, x.StdManHour, x.Note }).ToListAsync();

        var parts = await db.AppPartItems.Where(x => x.OrgId == Org && x.AppCode == a.Code)
            .OrderBy(x => x.PartCode)
            .Select(x => new { x.Id, x.PartCode, x.PartName, x.Unit, x.Quantity, x.InventoryQuantity, x.Note }).ToListAsync();
        var partRows = parts.Select(x => new
        {
            x.Id, x.PartCode, x.PartName, x.Unit, x.Quantity, x.InventoryQuantity, x.Note,
            shortage = x.Quantity > x.InventoryQuantity
        });

        // Khoang + KTV + RO gắn kèm (Ser_Cavity.CavityName / Ser_Engineer.EngineerName / Ser_RO.RONo).
        var bay = string.IsNullOrWhiteSpace(a.BayCode) ? null
            : await db.ServiceBays.Where(b => b.OrgId == Org && b.Code == a.BayCode)
                .Select(b => new { b.Code, b.Name, b.BayType }).FirstOrDefaultAsync();
        var engineer = string.IsNullOrWhiteSpace(a.Engineer) ? null
            : await db.Engineers.Where(e => e.OrgId == Org && e.Code == a.Engineer)
                .Select(e => new { e.Code, e.Name, e.Skill }).FirstOrDefaultAsync();
        var ro = string.IsNullOrWhiteSpace(a.RoId) ? null
            : await db.RepairOrders.Where(r => r.OrgId == Org && r.RoId == a.RoId)
                .Select(r => new { r.RoId, r.RoNo, r.Status }).FirstOrDefaultAsync();

        return new
        {
            a.Code, a.CustomerName, a.Phone, a.Vin, a.Plate, a.ServiceType,
            a.PreferredAt, a.SlotFrom, a.SlotTo, a.DealerCode,
            status = a.Status.ToString(), statusText = Text(a.Status),
            a.AppTypeCode, a.BayCode, a.Engineer, a.RoNo, a.RoId, a.Note,
            a.CreatedAt, a.ContactedAt, a.ContactResult, a.ContactNote, a.CheckedInAt, a.DoneAt,
            bay, engineer, ro,
            serviceItems = services, partItems = partRows,
            totalManHour = services.Sum(x => x.StdManHour),
            totalPartQuantity = parts.Sum(x => x.Quantity)
        };
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

    // ===== Nhắc chăm sóc sinh nhật KH (Ser_CustomerCareBth) =====
    public async Task<object> CreateBirthdayCareAsync(CreateBirthdayCareDto dto)
    {
        var careBthId = string.IsNullOrWhiteSpace(dto.CareBthId)
            ? "BTH" + DateTime.Now.ToString("yyMMddHHmmss") + Random.Shared.Next(10, 99)
            : dto.CareBthId!.Trim();
        var existing = await db.BirthdayCares.FirstOrDefaultAsync(x => x.OrgId == Org && x.CareBthId == careBthId);
        // Ser_CustomerCareBth: DateBth chuẩn hóa về năm hiện tại (29/02 → 28/02 nếu năm không nhuận).
        DateTime? dateBth = dto.DateBth.HasValue
            ? BirthdayCareStatuses.NormalizeToYear(dto.DateBth.Value, DateTime.Today.Year)
            : null;
        if (existing is null)
        {
            existing = new BirthdayCare { OrgId = Org, CareBthId = careBthId, Status = BirthdayCareStatuses.NotContacted };
            db.BirthdayCares.Add(existing);
        }
        existing.DealerCode = dto.DealerCode?.Trim() ?? existing.DealerCode;
        existing.CusId = dto.CusId.Trim();
        existing.CustomerName = dto.CustomerName.Trim();
        existing.Phone = dto.Phone?.Trim();
        existing.Email = dto.Email?.Trim();
        existing.Plate = dto.Plate?.Trim();
        existing.FrameNo = dto.FrameNo?.Trim();
        existing.TradeMarkCode = dto.TradeMarkCode?.Trim();
        existing.ModelName = dto.ModelName?.Trim();
        if (dateBth.HasValue) existing.DateBth = dateBth;
        if (!string.IsNullOrWhiteSpace(dto.Remark)) existing.Remark = dto.Remark;
        existing.LogLUDateTime = DateTime.Now;
        await db.SaveChangesAsync();
        return new { existing.Id, existing.CareBthId, existing.CustomerName, existing.DateBth, existing.Status, statusText = BirthdayCareStatuses.Text(existing.Status) };
    }

    // Ser_CustomerCareBth_Get_DL: tìm nhắc sinh nhật theo trạng thái/đại lý/tên KH/biển số/số khung/ngày sinh + phân trang.
    public async Task<object> ListBirthdayCaresAsync(string? status, string? dealer, string? customerName, string? plate, string? frameNo, string? dateBth, int? recordStart, int? recordCount)
    {
        var q = db.BirthdayCares.Where(x => x.OrgId == Org);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status.Trim());
        if (!string.IsNullOrWhiteSpace(dealer)) q = q.Where(x => x.DealerCode == dealer.Trim());
        if (!string.IsNullOrWhiteSpace(customerName)) q = q.Where(x => x.CustomerName.Contains(customerName.Trim()));
        if (!string.IsNullOrWhiteSpace(plate)) q = q.Where(x => x.Plate != null && x.Plate.Contains(plate.Trim()));
        if (!string.IsNullOrWhiteSpace(frameNo)) q = q.Where(x => x.FrameNo != null && x.FrameNo.Contains(frameNo.Trim()));
        if (!string.IsNullOrWhiteSpace(dateBth) && DateTime.TryParse(dateBth, out var d)) q = q.Where(x => x.DateBth != null && x.DateBth.Value.Date == d.Date);
        var total = await q.CountAsync();
        var start = Math.Max(0, recordStart ?? 0);
        var count = recordCount is > 0 ? recordCount!.Value : 100;
        var items = await q.OrderByDescending(x => x.DateBth).ThenBy(x => x.CustomerName)
            .Skip(start).Take(count)
            .Select(x => new { x.Id, x.CareBthId, x.DealerCode, x.CusId, x.CustomerName, x.Phone, x.Email, x.Plate, x.FrameNo, x.TradeMarkCode, x.ModelName, x.DateBth, x.Status, x.ContactDate, x.Remark, x.CreatedAt })
            .ToListAsync();
        return new { total, count = items.Count, recordStart = start, items = items.Select(x => new { x.Id, x.CareBthId, x.DealerCode, x.CusId, x.CustomerName, x.Phone, x.Email, x.Plate, x.FrameNo, x.TradeMarkCode, x.ModelName, x.DateBth, x.Status, statusText = BirthdayCareStatuses.Text(x.Status), x.ContactDate, x.Remark, x.CreatedAt }) };
    }

    public async Task<object?> GetBirthdayCareAsync(string careBthId)
    {
        var x = await db.BirthdayCares.FirstOrDefaultAsync(r => r.OrgId == Org && r.CareBthId == careBthId);
        if (x is null) return null;
        return new { x.Id, x.CareBthId, x.DealerCode, x.CusId, x.CustomerName, x.Phone, x.Email, x.Plate, x.FrameNo, x.TradeMarkCode, x.ModelName, x.DateBth, x.Status, statusText = BirthdayCareStatuses.Text(x.Status), x.ContactDate, x.Remark, x.CreatedAt, x.LogLUDateTime, x.LogLUBy };
    }

    // Ser_CustomerCareBth_Update: cập nhật trạng thái liên hệ/ngày liên hệ/ghi chú/ngày sinh (chặn khi không thấy CareBthId).
    public async Task<object?> UpdateBirthdayCareAsync(string careBthId, UpdateBirthdayCareDto dto)
    {
        var x = await db.BirthdayCares.FirstOrDefaultAsync(r => r.OrgId == Org && r.CareBthId == careBthId);
        if (x is null) return null;   // Ser_CustomerCareBth_NotFound
        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            var st = dto.Status!.Trim();
            if (!BirthdayCareStatuses.All.Contains(st))
                throw new InvalidOperationException($"Trạng thái '{st}' không hợp lệ (0=Chưa liên hệ, 1=Đã liên hệ, 2=Không liên hệ).");
            x.Status = st;
        }
        if (!string.IsNullOrWhiteSpace(dto.ContactDate) && DateTime.TryParse(dto.ContactDate, out var cd)) x.ContactDate = cd.Date;
        if (dto.Remark is not null) x.Remark = string.IsNullOrWhiteSpace(dto.Remark) ? null : dto.Remark;
        if (dto.DateBth.HasValue) x.DateBth = BirthdayCareStatuses.NormalizeToYear(dto.DateBth.Value, DateTime.Today.Year);
        x.LogLUDateTime = DateTime.Now;
        await db.SaveChangesAsync();
        return new { x.Id, x.CareBthId, x.Status, statusText = BirthdayCareStatuses.Text(x.Status), x.ContactDate, x.Remark, x.DateBth };
    }

    public async Task<object> BirthdayCareStatsAsync()
    {
        var q = db.BirthdayCares.Where(x => x.OrgId == Org);
        var today = DateTime.Today;
        return new
        {
            total = await q.CountAsync(),
            notContacted = await q.CountAsync(x => x.Status == BirthdayCareStatuses.NotContacted),
            contacted = await q.CountAsync(x => x.Status == BirthdayCareStatuses.Contacted),
            notContact = await q.CountAsync(x => x.Status == BirthdayCareStatuses.NotContact),
            // Sinh nhật trong 7 ngày tới (chưa liên hệ) — gợi ý danh sách cần gọi.
            upcoming7d = await q.CountAsync(x => x.Status == BirthdayCareStatuses.NotContacted && x.DateBth != null
                && x.DateBth.Value.Date >= today && x.DateBth.Value.Date <= today.AddDays(7))
        };
    }

    // ===== Khoang sửa chữa (Ser_Cavity) + sức chứa theo khung giờ =====
    public async Task<object> AddBayAsync(AddBayDto dto)
    {
        var code = dto.Code.Trim().ToUpperInvariant();
        // Ser_Cavity_Create: validate khoảng thời gian sử dụng (FinishUseDate không được trước StartUseDate).
        DateTime? startUse = ParseDate(dto.StartUseDate);
        DateTime? finishUse = ParseDate(dto.FinishUseDate);
        if (startUse.HasValue && finishUse.HasValue && finishUse.Value < startUse.Value)
            throw new InvalidOperationException("Ngày ngừng sử dụng (FinishUseDate) không được trước ngày bắt đầu sử dụng (StartUseDate).");
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
        // Ser_Cavity_Create: lưu khoảng thời gian sử dụng + trạng thái khoang.
        if (dto.StartUseDate != null) b.StartUseDate = startUse;
        if (dto.FinishUseDate != null) b.FinishUseDate = finishUse;
        if (dto.Status != null) b.Status = dto.Status.Trim();
        await db.SaveChangesAsync();
        var now = DateTime.Now;
        return new { b.Code, b.Name, b.BayType, b.CapacityPerSlot, b.DealerCode, b.Active, b.StartUseDate, b.FinishUseDate, b.Status, statusUse = BayUsageRules.UsageStatus(b.StartUseDate, b.FinishUseDate, now), statusUseText = BayUsageRules.Text(BayUsageRules.UsageStatus(b.StartUseDate, b.FinishUseDate, now)) };
    }

    // Ser_Cavity_Get_Status_DL: danh sách khoang + lọc theo trạng thái sử dụng (1 = đang dùng, 2 = ngưng/chưa dùng).
    public async Task<object> ListBaysAsync(string? dealer, string? statusUse = null)
    {
        var q = db.ServiceBays.Where(x => x.OrgId == Org);
        if (!string.IsNullOrWhiteSpace(dealer)) q = q.Where(x => x.DealerCode == dealer);
        var rows = await q.OrderBy(x => x.Code).ToListAsync();
        var now = DateTime.Now;
        var items = rows.Select(x => new
        {
            x.Code, x.Name, x.BayType, x.CapacityPerSlot, x.DealerCode, x.Active, x.Note,
            x.StartUseDate, x.FinishUseDate, x.Status,
            statusUse = BayUsageRules.UsageStatus(x.StartUseDate, x.FinishUseDate, now),
            statusUseText = BayUsageRules.Text(BayUsageRules.UsageStatus(x.StartUseDate, x.FinishUseDate, now))
        });
        // strStatusUseConditionList: 1 = đang sử dụng, 2 = ngưng sử dụng / chưa được sử dụng.
        if (!string.IsNullOrWhiteSpace(statusUse))
        {
            var want = statusUse.Trim();
            items = items.Where(x => x.statusUse == want);
        }
        var list = items.ToList();
        return new { count = list.Count, items = list };
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

    // ===== Tổ kỹ thuật (Ser_GroupRepair — "Quản lý tổ kỹ thuật") =====
    // Ser_GroupRepair_Create/Update: tạo/cập nhật master nhóm KTV theo xưởng.
    // Validate: GroupRNo/DealerCode/GroupRName không rỗng (CheckGroupRFieldEmpty);
    // mã tổ duy nhất trong 1 đại lý (CheckExistGroupRNo / CheckExistGroupRNoModify).
    public async Task<object> SaveRepairGroupAsync(SaveRepairGroupDto dto)
    {
        var groupRNo = (dto.GroupRNo ?? "").Trim().ToUpperInvariant();
        var dealerCode = (dto.DealerCode ?? "").Trim();
        var groupRName = (dto.GroupRName ?? "").Trim();
        if (!RepairGroupRules.HasRequiredFields(groupRNo, dealerCode, groupRName))
            throw new InvalidOperationException("Cần GroupRNo, DealerCode và GroupRName.");

        var now = DateTime.Now;
        var g = await db.RepairGroups.FirstOrDefaultAsync(x => x.OrgId == Org && x.DealerCode == dealerCode && x.GroupRNo == groupRNo);
        if (g is null)
        {
            g = new RepairGroup
            {
                OrgId = Org, DealerCode = dealerCode, GroupRNo = groupRNo, GroupRName = groupRName,
                Note = dto.Note, IsActive = dto.IsActive ?? true,
                CreatedAt = now, CreatedBy = dto.ChangedBy, LogLUDateTime = now, LogLUBy = dto.ChangedBy
            };
            db.RepairGroups.Add(g);
        }
        else
        {
            g.GroupRName = groupRName;
            g.Note = dto.Note;
            if (dto.IsActive.HasValue) g.IsActive = dto.IsActive.Value;
            g.LogLUDateTime = now; g.LogLUBy = dto.ChangedBy;
        }
        await db.SaveChangesAsync();
        return new { g.Id, g.DealerCode, g.GroupRNo, g.GroupRName, g.Note, g.IsActive, g.CreatedAt, g.LogLUDateTime };
    }

    // Ser_GroupRepair_Get_DL: tìm tổ kỹ thuật (lọc đại lý + từ khóa mã/tên + cờ hiệu lực + phân trang).
    public async Task<object> ListRepairGroupsAsync(string? dealer, string? keyword, bool? active, int? recordStart, int? recordCount)
    {
        var q = db.RepairGroups.Where(x => x.OrgId == Org);
        if (!string.IsNullOrWhiteSpace(dealer)) q = q.Where(x => x.DealerCode == dealer!.Trim());
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword!.Trim();
            q = q.Where(x => x.GroupRNo.Contains(kw) || x.GroupRName.Contains(kw));
        }
        if (active.HasValue) q = q.Where(x => x.IsActive == active.Value);

        var total = await q.CountAsync();
        var start = Math.Max(0, recordStart ?? 0);
        var count = recordCount is > 0 ? recordCount.Value : 50;
        var items = await q.OrderBy(x => x.DealerCode).ThenBy(x => x.GroupRNo)
            .Skip(start).Take(count)
            .Select(x => new { x.Id, x.DealerCode, x.GroupRNo, x.GroupRName, x.Note, x.IsActive, x.CreatedAt, x.LogLUDateTime })
            .ToListAsync();
        return new { total, recordStart = start, recordCount = count, count = items.Count, items };
    }

    // Chi tiết 1 tổ kỹ thuật theo Id.
    public async Task<object?> GetRepairGroupAsync(long id)
    {
        var g = await db.RepairGroups.FirstOrDefaultAsync(x => x.OrgId == Org && x.Id == id);
        if (g is null) return null;
        return new { g.Id, g.DealerCode, g.GroupRNo, g.GroupRName, g.Note, g.IsActive, g.CreatedAt, g.CreatedBy, g.LogLUDateTime, g.LogLUBy };
    }

    // Ser_GroupRepair_Delete: xóa tổ kỹ thuật (chặn khi không thấy — CheckExistGroupR).
    public async Task<object?> DeleteRepairGroupAsync(long id)
    {
        var g = await db.RepairGroups.FirstOrDefaultAsync(x => x.OrgId == Org && x.Id == id);
        if (g is null) return null;
        db.RepairGroups.Remove(g);
        await db.SaveChangesAsync();
        return new { g.Id, g.DealerCode, g.GroupRNo, g.GroupRName, deleted = true };
    }

    // ===== Lịch làm việc của xưởng (Mst_Calendar) =====
    // Mst_Calendar_ResetYear: xóa toàn bộ ngày của năm rồi sinh lại 1 dòng/ngày với StatusValue theo thứ.
    // StatusValue = 0 → ngày làm việc; khác 0 → ngày nghỉ/lễ. Year hợp lệ 1900..2100.
    public async Task<object> ResetCalendarYearAsync(ResetCalendarYearDto dto)
    {
        var calendarType = string.IsNullOrWhiteSpace(dto.CalendarType) ? CalendarTypes.WorkingDay : dto.CalendarType!.Trim().ToUpperInvariant();
        if (dto.Year < 1900 || dto.Year > 2100)
            throw new InvalidOperationException($"Năm {dto.Year} không hợp lệ (1900..2100).");

        var first = new DateTime(dto.Year, 1, 1);
        var next = new DateTime(dto.Year + 1, 1, 1);

        // Xóa dữ liệu cũ của năm (Clear Old Data).
        var olds = await db.CalendarDays.Where(x => x.OrgId == Org && x.CalendarType == calendarType
            && x.Date >= first && x.Date < next).ToListAsync();
        db.CalendarDays.RemoveRange(olds);

        // StatusValue theo từng thứ (mặc định 0 = làm việc nếu không truyền).
        var byDow = new Dictionary<DayOfWeek, int>
        {
            [DayOfWeek.Monday] = dto.Monday ?? CalendarTypes.Working,
            [DayOfWeek.Tuesday] = dto.Tuesday ?? CalendarTypes.Working,
            [DayOfWeek.Wednesday] = dto.Wednesday ?? CalendarTypes.Working,
            [DayOfWeek.Thursday] = dto.Thursday ?? CalendarTypes.Working,
            [DayOfWeek.Friday] = dto.Friday ?? CalendarTypes.Working,
            [DayOfWeek.Saturday] = dto.Saturday ?? CalendarTypes.Working,
            [DayOfWeek.Sunday] = dto.Sunday ?? CalendarTypes.Working,
        };
        var now = DateTime.Now;
        int created = 0;
        for (var d = first; d < next; d = d.AddDays(1))
        {
            db.CalendarDays.Add(new CalendarDay
            {
                OrgId = Org, CalendarType = calendarType, Date = d.Date,
                StatusValue = byDow[d.DayOfWeek], LogLUDateTime = now
            });
            created++;
        }
        await db.SaveChangesAsync();
        return new { calendarType, year = dto.Year, removed = olds.Count, created };
    }

    // Mst_Calendar_Get: danh sách ngày theo loại lịch + năm (hoặc khoảng from..to).
    public async Task<object> ListCalendarDaysAsync(string? calendarType, int? year, string? from, string? to)
    {
        var type = string.IsNullOrWhiteSpace(calendarType) ? CalendarTypes.WorkingDay : calendarType!.Trim().ToUpperInvariant();
        var q = db.CalendarDays.Where(x => x.OrgId == Org && x.CalendarType == type);
        if (year.HasValue)
        {
            var first = new DateTime(year.Value, 1, 1);
            var next = new DateTime(year.Value + 1, 1, 1);
            q = q.Where(x => x.Date >= first && x.Date < next);
        }
        if (!string.IsNullOrWhiteSpace(from) && DateTime.TryParse(from, out var f)) q = q.Where(x => x.Date >= f.Date);
        if (!string.IsNullOrWhiteSpace(to) && DateTime.TryParse(to, out var t)) q = q.Where(x => x.Date <= t.Date);
        var items = await q.OrderBy(x => x.Date).Take(1000)
            .Select(x => new { x.Date, x.StatusValue, isWorking = x.StatusValue == CalendarTypes.Working, x.LogLUDateTime, x.LogLUBy })
            .ToListAsync();
        return new { calendarType = type, count = items.Count, workingDays = items.Count(x => x.isWorking), items };
    }

    // Mst_Calendar_UpdateStatusValue: đổi trạng thái 1 ngày; ngày phải tồn tại (Mst_Calendar_CheckDB).
    public async Task<object?> UpdateCalendarDayAsync(UpdateCalendarDayDto dto)
    {
        var type = string.IsNullOrWhiteSpace(dto.CalendarType) ? CalendarTypes.WorkingDay : dto.CalendarType!.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(dto.Date) || !DateTime.TryParse(dto.Date, out var date))
            throw new InvalidOperationException("Cần Date hợp lệ (yyyy-MM-dd).");
        var day = await db.CalendarDays.FirstOrDefaultAsync(x => x.OrgId == Org && x.CalendarType == type && x.Date == date.Date);
        if (day is null)
            throw new InvalidOperationException($"Ngày {date:yyyy-MM-dd} chưa có trong lịch '{type}' (cần khởi tạo năm trước).");
        day.StatusValue = dto.StatusValue;
        day.LogLUDateTime = DateTime.Now;
        await db.SaveChangesAsync();
        return new { day.CalendarType, day.Date, day.StatusValue, isWorking = CalendarTypes.IsWorking(day.StatusValue), day.LogLUDateTime };
    }

    // Mst_Calendar_GetDateToCheck: ngày làm việc thứ N (dayOffset) kể từ mốc 'from' (chỉ đếm ngày StatusValue=0).
    public async Task<object?> NextWorkingDayAsync(string from, int dayOffset)
    {
        if (string.IsNullOrWhiteSpace(from) || !DateTime.TryParse(from, out var start))
            throw new InvalidOperationException("Cần 'from' hợp lệ (yyyy-MM-dd).");
        if (dayOffset < 0) dayOffset = 0;
        var working = await db.CalendarDays
            .Where(x => x.OrgId == Org && x.CalendarType == CalendarTypes.WorkingDay
                && x.StatusValue == CalendarTypes.Working && x.Date >= start.Date)
            .OrderBy(x => x.Date).Take(dayOffset + 1).ToListAsync();
        if (working.Count <= dayOffset) return null;   // không đủ ngày làm việc trong lịch đã khởi tạo
        var target = working[dayOffset];
        return new { from = start.Date, dayOffset, date = target.Date, isWorking = true };
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
                CusRequest = dto.CusRequest, Status = string.IsNullOrWhiteSpace(dto.Status) ? RoStages.Create : dto.Status!.Trim().ToUpperInvariant()
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
        return new { ro.RoId, ro.RoNo, ro.DealerCode, ro.CusName, ro.PlateNo, ro.Status, statusText = RoStages.Text(ro.Status), group = RoStages.Group(ro.Status), ro.AppCode, ro.LinkedAt };
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

    // Ser_RO_UpdateStatus: chuyển trạng thái lệnh sửa chữa theo máy trạng thái Ser_RO_Stage.
    // Chỉ cho phép các chuyển đổi hợp lệ (CRE→PRT→W4P→HPA→HRO→INGA→RPRD→CEND→PAID→FNS; nhánh REJ/NORE).
    public async Task<object?> ChangeRepairOrderStatusAsync(string roId, ChangeRoStatusDto dto)
    {
        roId = roId.Trim().ToUpperInvariant();
        var to = (dto.ToStatus ?? "").Trim().ToUpperInvariant();
        if (to.Length == 0) throw new InvalidOperationException("Cần ToStatus (trạng thái đích).");
        if (!RoStages.All.Contains(to))
            throw new InvalidOperationException($"Trạng thái '{to}' không hợp lệ. Hợp lệ: {string.Join('/', RoStages.All)}.");

        var ro = await db.RepairOrders.FirstOrDefaultAsync(x => x.OrgId == Org && x.RoId == roId);
        if (ro is null) return null;

        var from = (ro.Status ?? "").Trim().ToUpperInvariant();
        if (from == to)
            throw new InvalidOperationException($"Lệnh sửa chữa '{roId}' đã ở trạng thái {to} ({RoStages.Text(to)}).");
        if (!RoStages.CanTransition(from, to))
        {
            var allowed = RoStages.AllowedFrom(to);
            var allowedText = allowed.Length == 0 ? "(không có)" : string.Join('/', allowed);
            throw new InvalidOperationException(
                $"Không thể chuyển '{roId}' từ {from} ({RoStages.Text(from)}) sang {to} ({RoStages.Text(to)}). Trạng thái nguồn hợp lệ: {allowedText}.");
        }

        ro.Status = to;
        ro.StatusChangedAt = DateTime.Now;
        db.RepairOrderStatusHistories.Add(new RepairOrderStatusHistory
        {
            OrgId = Org, RoId = ro.RoId, FromStatus = from, ToStatus = to,
            Note = dto.Note, ChangedBy = dto.ChangedBy, ChangedAt = DateTime.Now
        });
        await db.SaveChangesAsync();
        return new { ro.RoId, fromStatus = from, status = ro.Status, statusText = RoStages.Text(ro.Status), group = RoStages.Group(ro.Status), ro.StatusChangedAt };
    }

    // SerROToRORejectStatusDL (SerROController.UpdateStatusToRejectRODL): hủy/từ chối lệnh sửa chữa.
    // Quy tắc:
    //  - ROID/RejectDate/RejectNote bắt buộc; RejectDate phải là ngày hợp lệ; RO phải tồn tại.
    //  - Nếu RO đã ở trạng thái REJ → lỗi (SerRO_ROIsReject).
    //  - Nếu RO đang RPRD/PAID/FNS/CEND → không cho hủy (SerRO_NotUpdateROReject).
    //  - Đặt Status = REJ, ghi lịch sử (Ser_ROHistory) và XÓA phân công công việc của RO
    //    (Ser_AssignmentWork + Ser_AssignmentWorkEngineer).
    public async Task<object?> RejectRepairOrderAsync(string roId, RejectRepairOrderDto dto)
    {
        roId = roId.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(dto.RejectDate) || !DateTime.TryParse(dto.RejectDate, out var rejectDate))
            throw new InvalidOperationException("Cần RejectDate (ngày hủy) hợp lệ.");
        if (string.IsNullOrWhiteSpace(dto.RejectNote))
            throw new InvalidOperationException("Cần RejectNote (lý do hủy).");

        var ro = await db.RepairOrders.FirstOrDefaultAsync(x => x.OrgId == Org && x.RoId == roId);
        if (ro is null) return null;

        var from = (ro.Status ?? "").Trim().ToUpperInvariant();
        if (from == RoStages.RejectRO)
            throw new InvalidOperationException($"Lệnh sửa chữa '{roId}' đã ở trạng thái Hủy / từ chối.");
        if (from is RoStages.Repaired or RoStages.Paid or RoStages.Finished or RoStages.CheckEnd)
            throw new InvalidOperationException(
                $"Lệnh sửa chữa '{roId}' đang ở {from} ({RoStages.Text(from)}), không thể hủy.");

        ro.Status = RoStages.RejectRO;
        ro.StatusChangedAt = DateTime.Now;
        db.RepairOrderStatusHistories.Add(new RepairOrderStatusHistory
        {
            OrgId = Org, RoId = ro.RoId, FromStatus = from, ToStatus = RoStages.RejectRO,
            Note = dto.RejectNote.Trim(), ChangedBy = dto.ChangedBy, ChangedAt = rejectDate
        });

        // Xóa phân công công việc của RO (Ser_AssignmentWork + Ser_AssignmentWorkEngineer).
        var wa = await db.WorkAssignments.FirstOrDefaultAsync(x => x.OrgId == Org && x.RoId == roId);
        int removedStages = 0, removedEngineers = 0;
        if (wa is not null)
        {
            var stages = await db.WorkAssignmentStages.Where(x => x.OrgId == Org && x.AssignmentId == wa.Id).ToListAsync();
            var engs = await db.WorkAssignmentEngineers.Where(x => x.OrgId == Org && x.AssignmentId == wa.Id).ToListAsync();
            removedStages = stages.Count; removedEngineers = engs.Count;
            db.WorkAssignmentStages.RemoveRange(stages);
            db.WorkAssignmentEngineers.RemoveRange(engs);
            db.WorkAssignments.Remove(wa);
        }

        await db.SaveChangesAsync();
        return new
        {
            ro.RoId, fromStatus = from, status = ro.Status, statusText = RoStages.Text(ro.Status),
            group = RoStages.Group(ro.Status), rejectDate, rejectNote = dto.RejectNote.Trim(),
            ro.StatusChangedAt, removedAssignment = wa is not null, removedStages, removedEngineers
        };
    }

    // Lịch sử đổi trạng thái của 1 lệnh sửa chữa (Ser_ROHistory).
    public async Task<object?> GetRepairOrderStatusHistoryAsync(string roId)
    {
        roId = roId.Trim().ToUpperInvariant();
        var ro = await db.RepairOrders.FirstOrDefaultAsync(x => x.OrgId == Org && x.RoId == roId);
        if (ro is null) return null;
        var items = await db.RepairOrderStatusHistories.Where(x => x.OrgId == Org && x.RoId == roId)
            .OrderBy(x => x.ChangedAt)
            .Select(x => new { x.Id, x.FromStatus, x.ToStatus, x.Note, x.ChangedBy, x.ChangedAt }).ToListAsync();
        var rows = items.Select(x => new
        {
            x.Id, x.FromStatus, x.ToStatus, x.Note, x.ChangedBy, x.ChangedAt,
            toStatusText = RoStages.Text(x.ToStatus)
        });
        return new { ro.RoId, status = ro.Status, statusText = RoStages.Text(ro.Status), count = items.Count, items = rows };
    }

    // Ser_RO_UpdatePlanedDeliveryDateDL: lưu ngày giao xe dự kiến của 1 lệnh sửa chữa.
    // Quy tắc (theo controller SerROController.UpdatePlanedDeliveryDateDL + BizCarSv.Service01):
    //  - ROID/PlanedDeliveryDate/Remark bắt buộc; RO phải tồn tại.
    //  - Ngày giao xe dự kiến phải SAU ngày vào xưởng (CheckInDate).
    //  - Ngày mới phải KHÁC ngày cũ (không lưu khi không đổi).
    //  - Không cho đổi khi RO đã Hoàn tất (FNS).
    //  - Ghi lịch sử: dòng cũ FlagCurrent=false, thêm dòng mới FlagCurrent=true (Ser_Ro_PlanedDeliveryDate_His).
    public async Task<object?> UpdatePlannedDeliveryDateAsync(string roId, UpdatePlannedDeliveryDateDto dto)
    {
        roId = roId.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(dto.PlanedDeliveryDate) || !DateTime.TryParse(dto.PlanedDeliveryDate, out var newDate))
            throw new InvalidOperationException("Cần PlanedDeliveryDate hợp lệ (ngày giao xe dự kiến).");
        if (string.IsNullOrWhiteSpace(dto.Remark))
            throw new InvalidOperationException("Cần Remark (lý do đổi ngày giao xe dự kiến).");

        var ro = await db.RepairOrders.FirstOrDefaultAsync(x => x.OrgId == Org && x.RoId == roId);
        if (ro is null) return null;

        // Không cho đổi khi RO đã hoàn tất (Ser_RO_Stage.Finished).
        if (string.Equals((ro.Status ?? "").Trim(), RoStages.Finished, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Lệnh sửa chữa '{roId}' đã hoàn tất ({RoStages.Text(RoStages.Finished)}), không thể đổi ngày giao xe dự kiến.");

        // Ngày giao xe dự kiến phải sau ngày vào xưởng (CheckInDate).
        if (ro.CheckInDate.HasValue && newDate <= ro.CheckInDate.Value)
            throw new InvalidOperationException($"Ngày giao xe dự kiến {newDate:yyyy-MM-dd HH:mm} phải sau ngày vào xưởng {ro.CheckInDate.Value:yyyy-MM-dd HH:mm}.");

        // Không lưu khi ngày không đổi.
        if (ro.PlanedDeliveryDate.HasValue && ro.PlanedDeliveryDate.Value == newDate)
            throw new InvalidOperationException($"Ngày giao xe dự kiến không thay đổi ({newDate:yyyy-MM-dd HH:mm}).");

        var now = DateTime.Now;
        // Đánh dấu các bản ghi cũ không còn hiện hành (FlagCurrent = false).
        var olds = await db.RepairOrderDeliveryPlans.Where(x => x.OrgId == Org && x.RoId == roId && x.FlagCurrent).ToListAsync();
        foreach (var o in olds) { o.FlagCurrent = false; o.LogLUDateTime = now; o.LogLUBy = dto.ChangedBy; }

        ro.PlanedDeliveryDate = newDate;
        db.RepairOrderDeliveryPlans.Add(new RepairOrderDeliveryPlan
        {
            OrgId = Org, RoId = roId, PlanedDeliveryDate = newDate, Remark = dto.Remark.Trim(),
            FlagCurrent = true, CreatedBy = dto.ChangedBy, CreatedDate = now, LogLUDateTime = now, LogLUBy = dto.ChangedBy
        });
        await db.SaveChangesAsync();
        return new { ro.RoId, ro.PlanedDeliveryDate, remark = dto.Remark.Trim(), ro.CheckInDate, superseded = olds.Count };
    }

    // Lịch sử ngày giao xe dự kiến của 1 lệnh sửa chữa (Ser_Ro_PlanedDeliveryDate_His).
    public async Task<object?> GetPlannedDeliveryDateHistoryAsync(string roId)
    {
        roId = roId.Trim().ToUpperInvariant();
        var ro = await db.RepairOrders.FirstOrDefaultAsync(x => x.OrgId == Org && x.RoId == roId);
        if (ro is null) return null;
        var items = await db.RepairOrderDeliveryPlans.Where(x => x.OrgId == Org && x.RoId == roId)
            .OrderByDescending(x => x.CreatedDate)
            .Select(x => new { x.Id, x.PlanedDeliveryDate, x.Remark, x.FlagCurrent, x.CreatedBy, x.CreatedDate }).ToListAsync();
        return new { ro.RoId, current = ro.PlanedDeliveryDate, count = items.Count, items };
    }

    // Ser_RO_Update_Maintance_DL: cập nhật thông tin nhắc bảo dưỡng kế tiếp của 1 lệnh sửa chữa.
    // Quy tắc (theo BizCarSv.Service01.Ser_RO_Update_Maintance_DL):
    //  - RO phải tồn tại (Ser_RO_Update_Maintance_RONotExist).
    //  - Không cho cập nhật khi RO đã Đã thanh toán (PAID) hoặc Đã hoàn thành (FNS)
    //    (Ser_RO_Update_Maintance_InvalidStatus).
    //  - Cập nhật Km/ReminderMaintanceDate/ReminderMaintanceKm/WorkDoneSoon/MemberNo + LogLUDateTime/LogLUBy.
    public async Task<object?> UpdateRoMaintenanceAsync(string roId, UpdateRoMaintenanceDto dto)
    {
        roId = roId.Trim().ToUpperInvariant();
        var ro = await db.RepairOrders.FirstOrDefaultAsync(x => x.OrgId == Org && x.RoId == roId);
        if (ro is null) return null;

        // Không cho cập nhật khi RO đã thanh toán/hoàn tất (Ser_RO_Update_Maintance_InvalidStatus).
        var status = (ro.Status ?? "").Trim().ToUpperInvariant();
        if (status == RoStages.Paid || status == RoStages.Finished)
            throw new InvalidOperationException($"Lệnh sửa chữa '{roId}' đang ở trạng thái {RoStages.Text(status)}, không thể cập nhật thông tin nhắc bảo dưỡng.");

        // Ngày nhắc bảo dưỡng (nếu có) phải hợp lệ.
        DateTime? reminderDate = null;
        if (!string.IsNullOrWhiteSpace(dto.ReminderMaintanceDate))
        {
            if (!DateTime.TryParse(dto.ReminderMaintanceDate, out var rd))
                throw new InvalidOperationException("ReminderMaintanceDate không hợp lệ (định dạng ngày).");
            reminderDate = rd.Date;
        }

        var now = DateTime.Now;
        ro.Km = dto.Km;
        ro.ReminderMaintanceDate = reminderDate;
        ro.ReminderMaintanceKm = dto.ReminderMaintanceKm;
        ro.WorkDoneSoon = dto.WorkDoneSoon?.Trim();
        ro.MemberNo = dto.MemberNo?.Trim();
        ro.LogLUDateTime = now;
        ro.LogLUBy = dto.ChangedBy;
        await db.SaveChangesAsync();

        return new
        {
            ro.RoId, ro.RoNo, ro.Status,
            ro.Km, ro.ReminderMaintanceDate, ro.ReminderMaintanceKm, ro.WorkDoneSoon, ro.MemberNo,
            ro.LogLUDateTime, ro.LogLUBy
        };
    }

    // Ser_RO_GetForSerAppDL: lấy dữ liệu 1 lệnh sửa chữa để TẠO LỊCH HẸN (Ser_App) từ RO.
    // Trả về header RO (CusID/CarID/ROID/RONo/CusRequest) + danh sách công việc + phụ tùng
    // (phụ tùng lấy Quantity = NEED — số lượng cần theo RO) để điền sẵn form đặt lịch.
    // Chặn khi RO đã gắn cuộc hẹn (Ser_RO.AppId) — không tạo trùng lịch cho cùng 1 RO.
    public async Task<object?> GetRepairOrderForAppointmentAsync(string roId)
    {
        roId = roId.Trim().ToUpperInvariant();
        var ro = await db.RepairOrders.FirstOrDefaultAsync(x => x.OrgId == Org && x.RoId == roId);
        if (ro is null) return null;

        var services = await db.RepairOrderServiceItems.Where(x => x.OrgId == Org && x.RoId == roId)
            .OrderBy(x => x.Id)
            .Select(x => new { x.SerCode, x.SerName, x.StdManHour, x.Note }).ToListAsync();

        // Phụ tùng của RO: dùng Quantity làm số lượng cần (NEED) để điền sẵn lịch hẹn.
        var parts = await db.AppPartItems.Where(x => x.OrgId == Org && x.AppCode == roId)
            .OrderBy(x => x.PartCode)
            .Select(x => new { x.PartCode, x.PartName, x.Unit, x.Quantity, x.InventoryQuantity, x.Note }).ToListAsync();

        return new
        {
            // Header RO — ánh xạ sang các trường Ser_App khi tạo lịch hẹn.
            ro.RoId, ro.RoNo, ro.DealerCode, ro.CusName, ro.CusTel, ro.PlateNo, ro.FrameNo, ro.CusRequest,
            ro.Status, statusText = RoStages.Text(ro.Status),
            // Cờ cho biết RO đã gắn cuộc hẹn chưa (Ser_RO.AppId) — nếu đã gắn thì không tạo lịch mới.
            linked = !string.IsNullOrWhiteSpace(ro.AppCode), ro.AppCode,
            serviceCount = services.Count,
            partCount = parts.Count,
            serviceItems = services,
            partItems = parts
        };
    }

    // ===== Công việc trong lệnh sửa chữa (Ser_ROServiceItems) =====
    // Thêm 1 dòng công việc vào RO; dedupe theo SerCode. Validate theo Ser_RO_CreateRODL:
    // SerCode/ROType/ExpenseType bắt buộc; BDD/PDI chỉ được ROREPAIR hoặc LOCAL.
    public async Task<object?> AddRoServiceItemAsync(string roId, AddRoServiceItemDto dto)
    {
        roId = roId.Trim().ToUpperInvariant();
        var ro = await db.RepairOrders.FirstOrDefaultAsync(x => x.OrgId == Org && x.RoId == roId);
        if (ro is null) return null;

        var serCode = (dto.SerCode ?? "").Trim();
        if (serCode.Length == 0) throw new InvalidOperationException("Cần SerCode (mã công việc).");
        var roType = (dto.ROType ?? "").Trim().ToUpperInvariant();
        if (roType.Length == 0) throw new InvalidOperationException("Cần ROType (loại công việc).");
        if (!WorkTypes.All.Contains(roType))
            throw new InvalidOperationException($"ROType '{roType}' không hợp lệ. Hợp lệ: {string.Join('/', WorkTypes.All)}.");
        var expenseType = (dto.ExpenseType ?? "").Trim().ToUpperInvariant();
        if (expenseType.Length == 0) throw new InvalidOperationException("Cần ExpenseType (đối tượng thanh toán).");
        if (!ExpenseTypes.All.Contains(expenseType))
            throw new InvalidOperationException($"ExpenseType '{expenseType}' không hợp lệ. Hợp lệ: {string.Join('/', ExpenseTypes.All)}.");
        // Ser_RO_Create_InvalidService_ExpenseType: BDD/PDI chỉ được ROREPAIR hoặc LOCAL.
        if ((roType == WorkTypes.BDD || roType == WorkTypes.PDI) && !ExpenseTypes.IsValidForService(expenseType))
            throw new InvalidOperationException($"Công việc {roType} chỉ được đối tượng thanh toán {ExpenseTypes.Repair} hoặc {ExpenseTypes.Local}.");

        var item = await db.RepairOrderServiceItems.FirstOrDefaultAsync(x => x.OrgId == Org && x.RoId == roId && x.SerCode == serCode);
        if (item is null)
        {
            item = new RepairOrderServiceItem { OrgId = Org, RoId = roId, SerCode = serCode };
            db.RepairOrderServiceItems.Add(item);
        }
        item.SerName = dto.SerName?.Trim() ?? item.SerName;
        item.ROType = roType;
        item.ExpenseType = expenseType;
        item.StdManHour = dto.StdManHour ?? item.StdManHour;
        item.Factor = dto.Factor ?? item.Factor;
        item.Price = dto.Price ?? item.Price;
        item.VAT = dto.VAT ?? item.VAT;
        item.FlagAccrual = dto.FlagAccrual ?? item.FlagAccrual;
        if (dto.Note != null) item.Note = dto.Note;
        await db.SaveChangesAsync();
        return new { item.Id, item.RoId, item.SerCode, item.SerName, item.ROType, item.ExpenseType, item.StdManHour, item.Factor, item.Price, item.VAT, item.FlagAccrual, item.Status };
    }

    // Danh sách công việc của 1 RO + tổng tiền theo đối tượng thanh toán + cờ ServiceStatus của RO.
    public async Task<object?> ListRoServiceItemsAsync(string roId)
    {
        roId = roId.Trim().ToUpperInvariant();
        var ro = await db.RepairOrders.FirstOrDefaultAsync(x => x.OrgId == Org && x.RoId == roId);
        if (ro is null) return null;
        var items = await db.RepairOrderServiceItems.Where(x => x.OrgId == Org && x.RoId == roId)
            .OrderBy(x => x.Id)
            .Select(x => new { x.Id, x.SerCode, x.SerName, x.ROType, x.ExpenseType, x.StdManHour, x.Factor, x.Price, x.VAT, x.FlagAccrual, x.Status, x.Note, x.StatusChangedAt, x.StatusChangedBy })
            .ToListAsync();
        var rows = items.Select(x => new
        {
            x.Id, x.SerCode, x.SerName, x.ROType, x.ExpenseType, x.StdManHour, x.Factor, x.Price, x.VAT, x.FlagAccrual, x.Status, x.Note, x.StatusChangedAt, x.StatusChangedBy,
            roTypeText = WorkStages.Text(x.ROType),
            amount = x.StdManHour * x.Factor * x.Price,
            amountVat = x.StdManHour * x.Factor * x.Price * (1 + x.VAT / 100m)
        }).ToList();
        return new
        {
            ro.RoId, ro.Status, statusText = RoStages.Text(ro.Status), ro.ServiceStatus,
            count = rows.Count,
            doneCount = rows.Count(x => x.Status),
            totalAmount = rows.Sum(x => x.amount),
            totalAmountVat = rows.Sum(x => x.amountVat),
            items = rows
        };
    }

    // Bỏ 1 dòng công việc khỏi RO; sau đó tính lại ServiceStatus của RO.
    public async Task<object?> RemoveRoServiceItemAsync(string roId, long itemId)
    {
        roId = roId.Trim().ToUpperInvariant();
        var ro = await db.RepairOrders.FirstOrDefaultAsync(x => x.OrgId == Org && x.RoId == roId);
        if (ro is null) return null;
        var item = await db.RepairOrderServiceItems.FirstOrDefaultAsync(x => x.OrgId == Org && x.RoId == roId && x.Id == itemId);
        if (item is null) return null;
        db.RepairOrderServiceItems.Remove(item);
        await db.SaveChangesAsync();
        await RecalcRoServiceStatusAsync(ro);
        return new { ro.RoId, removedItemId = itemId, ro.ServiceStatus };
    }

    // Ser_RO_Update_ServiceItemsStatusRODL: cập nhật trạng thái hoàn thành của các dòng công việc.
    // Sau khi cập nhật, nếu MỌI dòng của RO đã xong → Ser_RO.ServiceStatus = Active (true), ngược lại false.
    public async Task<object?> UpdateRoServiceItemsStatusAsync(string roId, UpdateRoServiceItemsStatusDto dto)
    {
        roId = roId.Trim().ToUpperInvariant();
        var ro = await db.RepairOrders.FirstOrDefaultAsync(x => x.OrgId == Org && x.RoId == roId);
        if (ro is null) return null;
        if (dto.Items is null || dto.Items.Count == 0)
            throw new InvalidOperationException("Cần danh sách Items (SerCode + Status).");

        var now = DateTime.Now;
        var items = await db.RepairOrderServiceItems.Where(x => x.OrgId == Org && x.RoId == roId).ToListAsync();
        var byCode = items.ToDictionary(x => x.SerCode, StringComparer.OrdinalIgnoreCase);
        int updated = 0;
        foreach (var row in dto.Items)
        {
            var code = (row.SerCode ?? "").Trim();
            if (code.Length == 0) continue;
            if (!byCode.TryGetValue(code, out var item))
                throw new InvalidOperationException($"Công việc '{code}' không thuộc lệnh sửa chữa '{roId}'.");
            item.Status = row.Status;
            item.StatusChangedAt = now;
            item.StatusChangedBy = dto.ChangedBy;
            updated++;
        }
        await db.SaveChangesAsync();
        await RecalcRoServiceStatusAsync(ro);
        return new
        {
            ro.RoId, updated, ro.ServiceStatus,
            doneCount = items.Count(x => x.Status), total = items.Count,
            allDone = ro.ServiceStatus
        };
    }

    // Tính lại Ser_RO.ServiceStatus: true khi mọi dòng công việc đã xong (hoặc RO chưa có dòng nào).
    private async Task RecalcRoServiceStatusAsync(RepairOrder ro)
    {
        var items = await db.RepairOrderServiceItems.Where(x => x.OrgId == Org && x.RoId == ro.RoId).ToListAsync();
        var allDone = items.Count == 0 || items.All(x => x.Status);
        if (ro.ServiceStatus != allDone)
        {
            ro.ServiceStatus = allDone;
            await db.SaveChangesAsync();
        }
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

    // ===== Chăm sóc KH sau dịch vụ 24h (Ser_CustomerCare24h) =====
    // Tạo phiếu khảo sát hài lòng sớm (24h) sau khi giao xe; dedupe theo CusCareId.
    public async Task<object> CreatePostCare24hAsync(CreatePostCare24hDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CusCareId)) throw new InvalidOperationException("Cần CusCareId (mã phiếu chăm sóc 24h).");
        var cusCareId = dto.CusCareId.Trim().ToUpperInvariant();
        var c = await db.PostServiceCares24h.FirstOrDefaultAsync(x => x.OrgId == Org && x.CusCareId == cusCareId);
        if (c is null)
        {
            c = new PostServiceCare24h
            {
                OrgId = Org, CusCareId = cusCareId,
                RoId = dto.RoId?.Trim().ToUpperInvariant(), OrderId = dto.OrderId?.Trim(),
                CustomerName = dto.CustomerName?.Trim() ?? "", Phone = dto.Phone?.Trim(),
                Plate = dto.Plate?.Trim(), FrameNo = dto.FrameNo?.Trim(),
                DealerCode = dto.DealerCode?.Trim() ?? "", FinishedDate24 = dto.FinishedDate24,
                Note = dto.Note, Status = SerCareStatuses.Pending
            };
            db.PostServiceCares24h.Add(c);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(dto.CustomerName)) c.CustomerName = dto.CustomerName!.Trim();
            if (dto.Phone != null) c.Phone = dto.Phone.Trim();
            if (dto.Plate != null) c.Plate = dto.Plate.Trim();
            if (dto.FrameNo != null) c.FrameNo = dto.FrameNo.Trim();
            if (!string.IsNullOrWhiteSpace(dto.DealerCode)) c.DealerCode = dto.DealerCode!.Trim();
            if (dto.RoId != null) c.RoId = dto.RoId.Trim().ToUpperInvariant();
            if (dto.OrderId != null) c.OrderId = dto.OrderId.Trim();
            if (dto.FinishedDate24.HasValue) c.FinishedDate24 = dto.FinishedDate24;
            if (dto.Note != null) c.Note = dto.Note;
        }
        await db.SaveChangesAsync();
        return new { c.CusCareId, c.RoId, c.OrderId, c.CustomerName, c.Status, c.FinishedDate24 };
    }

    // Danh sách phiếu chăm sóc 24h; lọc theo trạng thái/đại lý/mốc giao xe + tên KH/biển số/số khung (chứa).
    public async Task<object> ListPostCares24hAsync(string? status, string? dealer, string? dueBefore, string? customerName, string? plate, string? frameNo)
    {
        var q = db.PostServiceCares24h.Where(x => x.OrgId == Org);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status.ToUpperInvariant());
        if (!string.IsNullOrWhiteSpace(dealer)) q = q.Where(x => x.DealerCode == dealer);
        if (!string.IsNullOrWhiteSpace(dueBefore) && DateTime.TryParse(dueBefore, out var d)) q = q.Where(x => x.FinishedDate24 != null && x.FinishedDate24.Value.Date <= d.Date);
        if (!string.IsNullOrWhiteSpace(customerName)) q = q.Where(x => x.CustomerName.Contains(customerName));
        if (!string.IsNullOrWhiteSpace(plate)) q = q.Where(x => x.Plate != null && x.Plate.Contains(plate));
        if (!string.IsNullOrWhiteSpace(frameNo)) q = q.Where(x => x.FrameNo != null && x.FrameNo.Contains(frameNo));
        var items = await q.OrderBy(x => x.FinishedDate24).Take(500).Select(x => new
        {
            x.CusCareId, x.RoId, x.OrderId, x.CustomerName, x.Phone, x.Plate, x.FrameNo, x.DealerCode,
            x.FinishedDate24, x.Status, statusText = SerCareStatuses.Text(x.Status), x.ContactDate24, x.YourSatisfyQSv24, x.YourRIWN24, x.Note
        }).ToListAsync();
        return new { count = items.Count, items };
    }

    public async Task<object?> GetPostCare24hAsync(string cusCareId)
    {
        cusCareId = cusCareId.Trim().ToUpperInvariant();
        var c = await db.PostServiceCares24h.FirstOrDefaultAsync(x => x.OrgId == Org && x.CusCareId == cusCareId);
        if (c is null) return null;
        return new { c.CusCareId, c.RoId, c.OrderId, c.CustomerName, c.Phone, c.Plate, c.FrameNo, c.DealerCode, c.FinishedDate24, c.Status, statusText = SerCareStatuses.Text(c.Status), c.ContactDate24, c.FyourCSSH24, c.WFBasicNeeds24, c.YourCarProblem24, c.YourRIWN24, c.YourSatisfyQSv24, c.YourHopeOfOur24, c.Note, c.CreatedAt };
    }

    // Ghi nhận liên hệ + trả lời khảo sát: PEND → CIFB (đã phản hồi) nếu có câu trả lời, ngược lại CINFB (chưa phản hồi).
    public async Task<object?> ContactPostCare24hAsync(string cusCareId, PostCare24hContactDto dto)
    {
        cusCareId = cusCareId.Trim().ToUpperInvariant();
        var c = await db.PostServiceCares24h.FirstOrDefaultAsync(x => x.OrgId == Org && x.CusCareId == cusCareId);
        if (c is null || c.Status == SerCareStatuses.Reject) return null;
        if (!string.IsNullOrWhiteSpace(dto.FyourCSSH24)) c.FyourCSSH24 = dto.FyourCSSH24!.Trim();
        if (!string.IsNullOrWhiteSpace(dto.WFBasicNeeds24)) c.WFBasicNeeds24 = dto.WFBasicNeeds24!.Trim();
        if (!string.IsNullOrWhiteSpace(dto.YourCarProblem24)) c.YourCarProblem24 = dto.YourCarProblem24!.Trim();
        if (!string.IsNullOrWhiteSpace(dto.YourRIWN24)) c.YourRIWN24 = dto.YourRIWN24!.Trim();
        if (!string.IsNullOrWhiteSpace(dto.YourSatisfyQSv24)) c.YourSatisfyQSv24 = dto.YourSatisfyQSv24!.Trim();
        if (!string.IsNullOrWhiteSpace(dto.YourHopeOfOur24)) c.YourHopeOfOur24 = dto.YourHopeOfOur24!.Trim();
        if (dto.Note != null) c.Note = dto.Note;
        c.ContactDate24 = string.IsNullOrWhiteSpace(dto.ContactDate24) ? DateTime.Now : (DateTime.TryParse(dto.ContactDate24, out var cd) ? cd : DateTime.Now);
        var answered = !string.IsNullOrWhiteSpace(c.YourSatisfyQSv24) || !string.IsNullOrWhiteSpace(c.YourCarProblem24)
            || !string.IsNullOrWhiteSpace(c.YourRIWN24) || !string.IsNullOrWhiteSpace(c.FyourCSSH24)
            || !string.IsNullOrWhiteSpace(c.WFBasicNeeds24) || !string.IsNullOrWhiteSpace(c.YourHopeOfOur24);
        c.Status = answered ? SerCareStatuses.ContactedIFNoB : SerCareStatuses.ContactedINeedFB;
        await db.SaveChangesAsync();
        return new { c.CusCareId, c.Status, statusText = SerCareStatuses.Text(c.Status), c.ContactDate24, c.YourSatisfyQSv24, c.YourRIWN24 };
    }

    // Bỏ qua không cần liên hệ (REJ).
    public async Task<object?> RejectPostCare24hAsync(string cusCareId, string? note)
    {
        cusCareId = cusCareId.Trim().ToUpperInvariant();
        var c = await db.PostServiceCares24h.FirstOrDefaultAsync(x => x.OrgId == Org && x.CusCareId == cusCareId);
        if (c is null) return null;
        c.Status = SerCareStatuses.Reject;
        if (!string.IsNullOrWhiteSpace(note)) c.Note = note;
        await db.SaveChangesAsync();
        return new { c.CusCareId, c.Status, statusText = SerCareStatuses.Text(c.Status), c.Note };
    }

    public async Task<object> PostCare24hStatsAsync()
    {
        var q = db.PostServiceCares24h.Where(x => x.OrgId == Org);
        var today = DateTime.Now.Date;
        return new
        {
            total = await q.CountAsync(),
            pending = await q.CountAsync(x => x.Status == SerCareStatuses.Pending),
            contactedNoFeedback = await q.CountAsync(x => x.Status == SerCareStatuses.ContactedINeedFB),
            contactedFeedback = await q.CountAsync(x => x.Status == SerCareStatuses.ContactedIFNoB),
            rejected = await q.CountAsync(x => x.Status == SerCareStatuses.Reject),
            overdue = await q.CountAsync(x => x.Status == SerCareStatuses.Pending && x.FinishedDate24 != null && x.FinishedDate24.Value.Date < today)
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

    // ===== Tìm lịch hẹn "GetNew" + mở rộng chi tiết (Ser_App_GetNewDL) =====
    // Bộ lọc theo AppId/AppNo/CreatedDate/Creator/AppStatus/AppDateTime/PlateNo/CusName/DealerCode (đa giá trị '|'),
    // phân trang RecordStart/RecordCount, và tùy chọn kèm chi tiết (Ser_App + Ser_AppServiceItems + Ser_AppPartItems)
    // ngay trong kết quả (IsGet_Ser_App / IsGet_Ser_AppServiceItems / IsGet_Ser_AppPartItems).
    public async Task<object> GetNewAppointmentsAsync(GetNewAppointmentsDto dto)
    {
        var q = db.Appointments.Where(a => a.OrgId == Org);

        // AppIdList: '|'-separated → IN (...) trên mã lịch hẹn (Ser_App.AppId ↔ Appointment.Code).
        var appIds = SplitList(dto.AppIds);
        if (appIds.Length > 0) q = q.Where(a => appIds.Contains(a.Code));

        // DealerCodeList: '|'-separated → IN (...).
        var dealers = SplitList(dto.DealerCodes);
        if (dealers.Length > 0) q = q.Where(a => dealers.Contains(a.DealerCode));

        // PlateNoList: '|'-separated → IN (...) trên biển số.
        var plates = SplitList(dto.PlateNos);
        if (plates.Length > 0) q = q.Where(a => a.Plate != null && plates.Contains(a.Plate.ToUpper()));

        // AppNoList: '|'-separated → IN (...) trên mã lịch hẹn (AppNo ↔ Code).
        var appNos = SplitList(dto.AppNos);
        if (appNos.Length > 0) q = q.Where(a => appNos.Contains(a.Code));

        // CusNameList: '|'-separated → IN (...) trên tên khách (chứa, không phân biệt hoa thường theo DB).
        var names = SplitList(dto.CustomerNames);
        if (names.Length > 0) q = q.Where(a => names.Any(n => a.CustomerName.ToUpper().Contains(n)));

        // CreatedDateList: '|'-separated → IN (...) theo ngày tạo (so khớp ngày).
        var createdDates = SplitList(dto.CreatedDates).Select(x => DateTime.TryParse(x, out var d) ? d.Date : (DateTime?)null)
            .Where(d => d.HasValue).Select(d => d!.Value).ToArray();
        if (createdDates.Length > 0) q = q.Where(a => createdDates.Contains(a.CreatedAt.Date));

        // AppDateTimeList: '|'-separated → IN (...) theo ngày hẹn (AppDateTimeFrom ↔ PreferredAt).
        var appDates = SplitList(dto.AppDateTimes).Select(x => DateTime.TryParse(x, out var d) ? d.Date : (DateTime?)null)
            .Where(d => d.HasValue).Select(d => d!.Value).ToArray();
        if (appDates.Length > 0) q = q.Where(a => appDates.Contains(a.PreferredAt.Date));

        // AppStatusList: '|'-separated mã trạng thái nguồn (1..5) → map sang ApptStatus.
        var statuses = SplitList(dto.Statuses).Select(ParseSourceStatus).Where(s => s.HasValue).Select(s => s!.Value).ToArray();
        if (statuses.Length > 0) q = q.Where(a => statuses.Contains(a.Status));

        // CreatorList: '|'-separated → IN (...) trên người tạo (MiniBooking lưu Creator qua Engineer như proxy).
        var creators = SplitList(dto.Creators);
        if (creators.Length > 0) q = q.Where(a => a.Engineer != null && creators.Contains(a.Engineer.ToUpper()));

        var total = await q.CountAsync();

        // Phân trang (RecordStart 0-based, RecordCount mặc định 50, tối đa 500).
        var start = Math.Max(0, dto.RecordStart ?? 0);
        var count = Math.Clamp(dto.RecordCount ?? 50, 1, 500);
        var rows = await q.OrderBy(a => a.PreferredAt).ThenBy(a => a.Id)
            .Skip(start).Take(count).ToListAsync();

        // IsGet_Ser_App: mặc định true (trả thông tin lịch hẹn).
        var includeApp = dto.IncludeApp ?? true;
        var includeServices = dto.IncludeServiceItems ?? false;
        var includeParts = dto.IncludePartItems ?? false;

        var codes = rows.Select(a => a.Code).ToArray();
        var services = includeServices
            ? await db.AppServiceItems.Where(x => x.OrgId == Org && codes.Contains(x.AppCode)).ToListAsync()
            : new List<AppServiceItem>();
        var parts = includeParts
            ? await db.AppPartItems.Where(x => x.OrgId == Org && codes.Contains(x.AppCode)).ToListAsync()
            : new List<AppPartItem>();

        var items = rows.Select(a => new
        {
            a.Code, a.CustomerName, a.Phone, a.Vin, a.Plate, a.ServiceType, a.PreferredAt,
            a.DealerCode, a.Engineer, status = a.Status.ToString(), statusText = Text(a.Status),
            a.RoNo, a.BayCode, a.AppTypeCode, a.SlotFrom, a.SlotTo, a.CreatedAt, a.ContactedAt, a.ContactResult,
            serviceItems = includeServices
                ? services.Where(s => s.AppCode == a.Code).Select(s => new { s.Id, s.SerCode, s.SerName, s.StdManHour, s.Note }).ToList()
                : null,
            partItems = includeParts
                ? parts.Where(p => p.AppCode == a.Code).Select(p => new { p.Id, p.PartCode, p.PartName, p.Unit, p.Quantity, p.InventoryQuantity, p.Note }).ToList()
                : null
        }).ToList();

        return new
        {
            total, recordStart = start, recordCount = count, count = items.Count,
            includeApp, includeServiceItems = includeServices, includePartItems = includeParts, items
        };
    }

    // ===== Tìm lịch hẹn để xếp khoang (Ser_App_GetForCavityDL) =====
    // Lọc theo biển số (chứa), 1 ngày cụ thể (AppDateTime), và 4 cờ loại cuộc hẹn:
    // BDDK (bảo dưỡng định kỳ) / SCC (sửa chữa chung) / SCDS (sửa chữa đồng sơn) / SCK (sửa chữa khác).
    // Cờ nào bật thì gom mã tương ứng vào danh sách AppTypeCode (IN ...); không bật cờ nào → không lọc theo loại.
    public async Task<object> GetForCavityAsync(GetForCavityDto dto)
    {
        var q = db.Appointments.Where(a => a.OrgId == Org);

        // PlateNoParttern: LIKE (chứa) trên biển số.
        if (!string.IsNullOrWhiteSpace(dto.PlateNo))
        {
            var p = dto.PlateNo.Trim();
            q = q.Where(a => a.Plate != null && a.Plate.Contains(p));
        }

        // DateTimeLine: lọc theo đúng 1 ngày hẹn (AppDateTime).
        if (!string.IsNullOrWhiteSpace(dto.DateTimeLine) && DateTime.TryParse(dto.DateTimeLine, out var day))
            q = q.Where(a => a.PreferredAt.Date == day.Date);

        // AppTypeCodeList: gom từ 4 cờ BDDK/SCC/SCDS/SCK → IN (...).
        var appTypes = new List<string>();
        if (dto.FlagBDDK == true) appTypes.Add("BDDK");
        if (dto.FlagSCC == true) appTypes.Add("SCC");
        if (dto.FlagSCDS == true) appTypes.Add("SCDS");
        if (dto.FlagSCK == true) appTypes.Add("SCK");
        if (appTypes.Count > 0)
        {
            var set = appTypes.ToArray();
            q = q.Where(a => a.AppTypeCode != null && set.Contains(a.AppTypeCode));
        }

        var total = await q.CountAsync();
        var items = await q.OrderBy(a => a.PreferredAt).ThenBy(a => a.Id).Take(500)
            .Select(a => new
            {
                a.Code, a.CustomerName, a.Phone, a.Vin, a.Plate, a.ServiceType, a.PreferredAt,
                a.DealerCode, a.Engineer, status = a.Status.ToString(), statusText = Text(a.Status),
                a.RoNo, a.BayCode, a.AppTypeCode, a.SlotFrom, a.SlotTo
            }).ToListAsync();

        return new { total, count = items.Count, appTypeCodes = appTypes, items };
    }

    private static string[] SplitList(string? raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? Array.Empty<string>()
            : raw.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                 .Select(x => x.ToUpperInvariant()).ToArray();

    // Parse ngày tùy chọn (trả null nếu trống/không hợp lệ) — dùng cho khoảng thời gian sử dụng khoang (Ser_Cavity).
    private static DateTime? ParseDate(string? raw) =>
        string.IsNullOrWhiteSpace(raw) ? null : (DateTime.TryParse(raw, out var d) ? d : null);

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

    // ===== Gói dịch vụ (Ser_ServicePackage) =====
    // Tạo gói dịch vụ: nhóm sẵn công việc + phụ tùng theo 1 giá gói. Dedupe theo (DealerCode, PackageNo).
    // Rule Ser_ServicePackage_Create: PackageNo/DealerCode/PackageName bắt buộc; phải có ít nhất 1 dòng dịch vụ;
    // mỗi dòng dịch vụ cần SerCode + ExpenseType (ROREPAIR/LOCAL) + ROType; dòng phụ tùng cần PartCode + ExpenseType hợp lệ.
    public async Task<object> CreateServicePackageAsync(CreateServicePackageDto dto)
    {
        var packageNo = dto.PackageNo?.Trim().ToUpperInvariant() ?? "";
        var dealerCode = dto.DealerCode?.Trim() ?? "";
        var packageName = dto.PackageName?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(packageNo)) throw new InvalidOperationException("Cần PackageNo (mã gói dịch vụ).");
        if (packageNo.Length > 50) throw new InvalidOperationException("Mã gói dịch vụ vượt quá 50 ký tự.");
        if (string.IsNullOrWhiteSpace(dealerCode)) throw new InvalidOperationException("Cần DealerCode (mã đại lý).");
        if (string.IsNullOrWhiteSpace(packageName)) throw new InvalidOperationException("Cần PackageName (tên gói dịch vụ).");
        if (packageName.Length > 200) throw new InvalidOperationException("Tên gói dịch vụ vượt quá 200 ký tự.");
        if (!string.IsNullOrWhiteSpace(dto.Description) && dto.Description!.Length > 500)
            throw new InvalidOperationException("Mô tả vượt quá 500 ký tự.");

        // Ser_ServicePackageNo_Exist: mã gói không được trùng trong cùng đại lý.
        var dup = await db.ServicePackages.AnyAsync(x => x.OrgId == Org && x.DealerCode == dealerCode && x.PackageNo == packageNo);
        if (dup) throw new InvalidOperationException($"Mã gói dịch vụ '{packageNo}' đã tồn tại cho đại lý '{dealerCode}'.");

        var serviceItems = ValidatePackageServiceItems(dto.ServiceItems);
        var partItems = ValidatePackagePartItems(dto.PartItems);

        var pkg = new ServicePackage
        {
            OrgId = Org, DealerCode = dealerCode, PackageNo = packageNo, PackageName = packageName,
            TakingTime = dto.TakingTime?.Trim(), Description = dto.Description?.Trim(), Creator = dto.Creator?.Trim(),
            IsPublicFlag = dto.IsPublicFlag ?? false, IsUserBasePrice = dto.IsUserBasePrice ?? false
        };
        db.ServicePackages.Add(pkg);
        await db.SaveChangesAsync();   // lấy pkg.Id

        foreach (var s in serviceItems)
            db.ServicePackageServiceItems.Add(new ServicePackageServiceItem
            {
                OrgId = Org, PackageId = pkg.Id, SerCode = s.SerCode, SerName = s.SerName,
                Factor = s.Factor, ActManHour = s.ActManHour, VAT = s.VAT, Price = s.Price,
                ExpenseType = s.ExpenseType, ROType = s.ROType, Note = s.Note
            });
        foreach (var p in partItems)
            db.ServicePackagePartItems.Add(new ServicePackagePartItem
            {
                OrgId = Org, PackageId = pkg.Id, PartCode = p.PartCode, PartName = p.PartName, Unit = p.Unit,
                Factor = p.Factor, Quantity = p.Quantity, VAT = p.VAT, Price = p.Price, ExpenseType = p.ExpenseType, Note = p.Note
            });
        await db.SaveChangesAsync();
        return await BuildPackageViewAsync(pkg);
    }

    // Danh sách gói dịch vụ; keyword tìm theo mã/tên; isPublic lọc theo cờ phạm vi.
    public async Task<object> ListServicePackagesAsync(string? dealer, string? keyword, bool? isPublic)
    {
        var q = db.ServicePackages.Where(x => x.OrgId == Org);
        if (!string.IsNullOrWhiteSpace(dealer)) q = q.Where(x => x.DealerCode == dealer);
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            q = q.Where(x => x.PackageNo.Contains(k) || x.PackageName.Contains(k));
        }
        if (isPublic.HasValue) q = q.Where(x => x.IsPublicFlag == isPublic.Value);
        var pkgs = await q.OrderBy(x => x.PackageNo).Take(500).ToListAsync();
        var ids = pkgs.Select(x => x.Id).ToArray();
        var svcCounts = await db.ServicePackageServiceItems.Where(x => x.OrgId == Org && ids.Contains(x.PackageId))
            .GroupBy(x => x.PackageId).Select(g => new { g.Key, c = g.Count() }).ToListAsync();
        var partCounts = await db.ServicePackagePartItems.Where(x => x.OrgId == Org && ids.Contains(x.PackageId))
            .GroupBy(x => x.PackageId).Select(g => new { g.Key, c = g.Count() }).ToListAsync();
        var items = pkgs.Select(p => new
        {
            p.Id, p.PackageNo, p.PackageName, p.DealerCode, p.TakingTime, p.Description, p.Creator,
            p.IsPublicFlag, p.IsUserBasePrice, p.CreatedAt, p.UpdatedAt,
            serviceItemCount = svcCounts.FirstOrDefault(x => x.Key == p.Id)?.c ?? 0,
            partItemCount = partCounts.FirstOrDefault(x => x.Key == p.Id)?.c ?? 0
        });
        return new { count = pkgs.Count, items };
    }

    public async Task<object?> GetServicePackageAsync(long id)
    {
        var pkg = await db.ServicePackages.FirstOrDefaultAsync(x => x.OrgId == Org && x.Id == id);
        return pkg is null ? null : await BuildPackageViewAsync(pkg);
    }

    // Sửa gói dịch vụ: cập nhật header + thay toàn bộ danh sách dịch vụ & phụ tùng (delete olds → insert).
    public async Task<object?> UpdateServicePackageAsync(long id, UpdateServicePackageDto dto)
    {
        var pkg = await db.ServicePackages.FirstOrDefaultAsync(x => x.OrgId == Org && x.Id == id);
        if (pkg is null) return null;
        if (!string.IsNullOrWhiteSpace(dto.PackageName))
        {
            if (dto.PackageName!.Trim().Length > 200) throw new InvalidOperationException("Tên gói dịch vụ vượt quá 200 ký tự.");
            pkg.PackageName = dto.PackageName.Trim();
        }
        if (!string.IsNullOrWhiteSpace(dto.DealerCode)) pkg.DealerCode = dto.DealerCode!.Trim();
        if (dto.TakingTime != null) pkg.TakingTime = dto.TakingTime.Trim();
        if (dto.Description != null)
        {
            if (dto.Description.Length > 500) throw new InvalidOperationException("Mô tả vượt quá 500 ký tự.");
            pkg.Description = dto.Description.Trim();
        }
        if (dto.IsPublicFlag.HasValue) pkg.IsPublicFlag = dto.IsPublicFlag.Value;
        if (dto.IsUserBasePrice.HasValue) pkg.IsUserBasePrice = dto.IsUserBasePrice.Value;
        pkg.UpdatedAt = DateTime.Now;

        if (dto.ServiceItems is not null)
        {
            var serviceItems = ValidatePackageServiceItems(dto.ServiceItems);
            var olds = await db.ServicePackageServiceItems.Where(x => x.OrgId == Org && x.PackageId == pkg.Id).ToListAsync();
            db.ServicePackageServiceItems.RemoveRange(olds);
            foreach (var s in serviceItems)
                db.ServicePackageServiceItems.Add(new ServicePackageServiceItem
                {
                    OrgId = Org, PackageId = pkg.Id, SerCode = s.SerCode, SerName = s.SerName,
                    Factor = s.Factor, ActManHour = s.ActManHour, VAT = s.VAT, Price = s.Price,
                    ExpenseType = s.ExpenseType, ROType = s.ROType, Note = s.Note
                });
        }
        if (dto.PartItems is not null)
        {
            var partItems = ValidatePackagePartItems(dto.PartItems);
            var olds = await db.ServicePackagePartItems.Where(x => x.OrgId == Org && x.PackageId == pkg.Id).ToListAsync();
            db.ServicePackagePartItems.RemoveRange(olds);
            foreach (var p in partItems)
                db.ServicePackagePartItems.Add(new ServicePackagePartItem
                {
                    OrgId = Org, PackageId = pkg.Id, PartCode = p.PartCode, PartName = p.PartName, Unit = p.Unit,
                    Factor = p.Factor, Quantity = p.Quantity, VAT = p.VAT, Price = p.Price, ExpenseType = p.ExpenseType, Note = p.Note
                });
        }
        await db.SaveChangesAsync();
        return await BuildPackageViewAsync(pkg);
    }

    // Xóa gói dịch vụ + toàn bộ dòng dịch vụ/phụ tùng của gói (Ser_ServicePackage_Delete).
    public async Task<object?> DeleteServicePackageAsync(long id)
    {
        var pkg = await db.ServicePackages.FirstOrDefaultAsync(x => x.OrgId == Org && x.Id == id);
        if (pkg is null) return null;
        var svc = await db.ServicePackageServiceItems.Where(x => x.OrgId == Org && x.PackageId == pkg.Id).ToListAsync();
        var parts = await db.ServicePackagePartItems.Where(x => x.OrgId == Org && x.PackageId == pkg.Id).ToListAsync();
        db.ServicePackageServiceItems.RemoveRange(svc);
        db.ServicePackagePartItems.RemoveRange(parts);
        db.ServicePackages.Remove(pkg);
        await db.SaveChangesAsync();
        return new { pkg.Id, pkg.PackageNo, removedServiceItems = svc.Count, removedPartItems = parts.Count };
    }

    // Chuẩn hóa + kiểm tra danh sách dòng dịch vụ của gói (SerServicePackageCreate_*).
    private static List<ServicePackageServiceItem> ValidatePackageServiceItems(List<PackageServiceItemDto>? items)
    {
        var list = new List<ServicePackageServiceItem>();
        if (items is null || items.Count == 0)
            throw new InvalidOperationException("Gói dịch vụ phải có ít nhất 1 dòng dịch vụ (SerServicePackageCreate_ServiceTableNotBlank).");
        var seen = new HashSet<string>();
        foreach (var i in items)
        {
            if (string.IsNullOrWhiteSpace(i.SerCode))
                throw new InvalidOperationException("Dòng dịch vụ thiếu SerCode (SerServicePackageCreate_ServiceNotInList).");
            var serCode = i.SerCode.Trim().ToUpperInvariant();
            if (!seen.Add(serCode)) continue;   // dedupe theo SerCode
            var expenseType = i.ExpenseType?.Trim().ToUpperInvariant() ?? "";
            if (string.IsNullOrWhiteSpace(expenseType))
                throw new InvalidOperationException($"Dòng dịch vụ '{serCode}' thiếu ExpenseType (đối tượng thanh toán).");
            if (!ExpenseTypes.IsValidForService(expenseType))
                throw new InvalidOperationException($"Dòng dịch vụ '{serCode}' có ExpenseType '{expenseType}' không hợp lệ (chỉ ROREPAIR/LOCAL).");
            var roType = i.ROType?.Trim().ToUpperInvariant() ?? "";
            if (string.IsNullOrWhiteSpace(roType))
                throw new InvalidOperationException($"Dòng dịch vụ '{serCode}' thiếu ROType (loại công việc).");
            // SerServicePackageCreate_Invalid_ExpenseType: công việc BDD chỉ được ROREPAIR/LOCAL.
            if (roType == WorkTypes.BDD && !ExpenseTypes.IsValidForService(expenseType))
                throw new InvalidOperationException($"Công việc BDD '{serCode}' chỉ được ExpenseType ROREPAIR/LOCAL.");
            list.Add(new ServicePackageServiceItem
            {
                SerCode = serCode, SerName = string.IsNullOrWhiteSpace(i.SerName) ? serCode : i.SerName!.Trim(),
                Factor = i.Factor is > 0 ? i.Factor!.Value : 1m,
                ActManHour = i.ActManHour is > 0 ? i.ActManHour!.Value : 0m,
                VAT = i.VAT is > 0 ? i.VAT!.Value : 0m,
                Price = i.Price is > 0 ? i.Price!.Value : 0m,
                ExpenseType = expenseType, ROType = roType, Note = i.Note
            });
        }
        return list;
    }

    // Chuẩn hóa + kiểm tra danh sách dòng phụ tùng của gói (SerServicePackageCreate_Part*).
    private static List<ServicePackagePartItem> ValidatePackagePartItems(List<PackagePartItemDto>? items)
    {
        var list = new List<ServicePackagePartItem>();
        if (items is null) return list;
        var seen = new HashSet<string>();
        foreach (var i in items)
        {
            if (string.IsNullOrWhiteSpace(i.PartCode))
                throw new InvalidOperationException("Dòng phụ tùng thiếu PartCode (SerServicePackageCreate_PartNotInStock).");
            var partCode = i.PartCode.Trim().ToUpperInvariant();
            if (!seen.Add(partCode)) continue;   // dedupe theo PartCode
            var expenseType = i.ExpenseType?.Trim().ToUpperInvariant() ?? "";
            if (string.IsNullOrWhiteSpace(expenseType))
                throw new InvalidOperationException($"Dòng phụ tùng '{partCode}' thiếu ExpenseType (đối tượng thanh toán).");
            if (!ExpenseTypes.IsValidForPart(expenseType))
                throw new InvalidOperationException($"Dòng phụ tùng '{partCode}' có ExpenseType '{expenseType}' không hợp lệ (ROREPAIR/LOCAL/ROINSURANCE/ROWARRANTY).");
            list.Add(new ServicePackagePartItem
            {
                PartCode = partCode, PartName = string.IsNullOrWhiteSpace(i.PartName) ? partCode : i.PartName!.Trim(),
                Unit = i.Unit?.Trim() ?? "",
                Factor = i.Factor is > 0 ? i.Factor!.Value : 1m,
                Quantity = i.Quantity is > 0 ? i.Quantity!.Value : 0m,
                VAT = i.VAT is > 0 ? i.VAT!.Value : 0m,
                Price = i.Price is > 0 ? i.Price!.Value : 0m,
                ExpenseType = expenseType, Note = i.Note
            });
        }
        return list;
    }

    // Dựng view 1 gói dịch vụ: header + dòng dịch vụ + dòng phụ tùng + tổng tiền (Amount = Price*Factor*Quantity).
    private async Task<object> BuildPackageViewAsync(ServicePackage pkg)
    {
        var svc = await db.ServicePackageServiceItems.Where(x => x.OrgId == Org && x.PackageId == pkg.Id)
            .OrderBy(x => x.SerCode).ToListAsync();
        var parts = await db.ServicePackagePartItems.Where(x => x.OrgId == Org && x.PackageId == pkg.Id)
            .OrderBy(x => x.PartCode).ToListAsync();
        var svcViews = svc.Select(s => new
        {
            s.Id, s.SerCode, s.SerName, s.Factor, s.ActManHour, s.VAT, s.Price, s.ExpenseType, s.ROType, s.Note,
            amount = Math.Round(s.Price * s.Factor, 2)
        });
        var partViews = parts.Select(p => new
        {
            p.Id, p.PartCode, p.PartName, p.Unit, p.Factor, p.Quantity, p.VAT, p.Price, p.ExpenseType, p.Note,
            amount = Math.Round(p.Price * p.Factor * p.Quantity, 2)
        });
        var serviceAmount = svc.Sum(s => s.Price * s.Factor);
        var partAmount = parts.Sum(p => p.Price * p.Factor * p.Quantity);
        return new
        {
            pkg.Id, pkg.PackageNo, pkg.PackageName, pkg.DealerCode, pkg.TakingTime, pkg.Description, pkg.Creator,
            pkg.IsPublicFlag, pkg.IsUserBasePrice, pkg.CreatedAt, pkg.UpdatedAt,
            serviceItemCount = svc.Count, partItemCount = parts.Count,
            serviceAmount = Math.Round(serviceAmount, 2), partAmount = Math.Round(partAmount, 2),
            totalAmount = Math.Round(serviceAmount + partAmount, 2),
            serviceItems = svcViews, partItems = partViews
        };
    }

    // ---- Chiến dịch marketing (Ser_CampaignMarketing) ----

    // Tạo/cập nhật chiến dịch marketing + điều kiện áp dụng + phụ tùng khuyến mãi (Ser_CampaignMarketing_Create/Update).
    public async Task<object> CreateCampaignAsync(CreateCampaignDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CamMarketingName))
            throw new InvalidOperationException("Cần CamMarketingName (tên chiến dịch).");
        var no = string.IsNullOrWhiteSpace(dto.CamMarketingNo)
            ? "CM" + DateTime.Now.ToString("yyMMddHHmmss") + Random.Shared.Next(10, 99)
            : dto.CamMarketingNo!.Trim();
        var status = string.IsNullOrWhiteSpace(dto.CamMarketingStatus) ? CampaignStatuses.Approve : dto.CamMarketingStatus!.Trim().ToUpperInvariant();
        if (dto.EffDateStart.HasValue && dto.EffDateEnd.HasValue && dto.EffDateEnd.Value < dto.EffDateStart.Value)
            throw new InvalidOperationException("EffDateEnd phải sau EffDateStart.");
        if (dto.WarrantyDateStart.HasValue && dto.WarrantyDateEnd.HasValue && dto.WarrantyDateEnd.Value < dto.WarrantyDateStart.Value)
            throw new InvalidOperationException("WarrantyDateEnd phải sau WarrantyDateStart.");

        var camp = await db.CampaignMarketings.FirstOrDefaultAsync(x => x.OrgId == Org && x.CamMarketingNo == no);
        if (camp is null)
        {
            camp = new CampaignMarketing { OrgId = Org, CamMarketingNo = no };
            db.CampaignMarketings.Add(camp);
        }
        camp.CamMarketingName = dto.CamMarketingName.Trim();
        camp.Description = dto.Description;
        camp.CamMarketingStatus = status;
        camp.EffDateStart = dto.EffDateStart;
        camp.EffDateEnd = dto.EffDateEnd;
        camp.WarrantyDateStart = dto.WarrantyDateStart;
        camp.WarrantyDateEnd = dto.WarrantyDateEnd;
        camp.ConditionPlateNo = dto.ConditionPlateNo ?? false;
        camp.ConditionDealer = dto.ConditionDealer ?? false;
        camp.ConditionVIN = dto.ConditionVIN ?? false;
        camp.ConditionFullVIN = dto.ConditionFullVIN ?? false;

        // Thay toàn bộ điều kiện + phụ tùng (giống Ser_CampaignMarketing_Update ghi đè danh sách con).
        var oldConds = await db.CampaignMarketingConditions.Where(x => x.OrgId == Org && x.CamMarketingNo == no).ToListAsync();
        var oldParts = await db.CampaignMarketingParts.Where(x => x.OrgId == Org && x.CamMarketingNo == no).ToListAsync();
        db.CampaignMarketingConditions.RemoveRange(oldConds);
        db.CampaignMarketingParts.RemoveRange(oldParts);

        if (dto.Conditions is not null)
            foreach (var c in dto.Conditions)
            {
                if (string.IsNullOrWhiteSpace(c.Value)) continue;
                var type = (c.ConditionType ?? "").Trim();
                if (!CampaignConditionTypes.All.Contains(type, StringComparer.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"ConditionType '{type}' không hợp lệ (chỉ PlateNo/Dealer/VIN/FullVIN).");
                db.CampaignMarketingConditions.Add(new CampaignMarketingCondition
                { OrgId = Org, CamMarketingNo = no, ConditionType = type, Value = c.Value.Trim() });
            }

        if (dto.Parts is not null)
        {
            var seen = new HashSet<string>();
            foreach (var p in dto.Parts)
            {
                if (string.IsNullOrWhiteSpace(p.PartCode)) continue;
                var partCode = p.PartCode.Trim().ToUpperInvariant();
                if (!seen.Add(partCode)) continue;   // dedupe theo PartCode
                db.CampaignMarketingParts.Add(new CampaignMarketingPart
                {
                    OrgId = Org, CamMarketingNo = no, PartCode = partCode,
                    PartName = string.IsNullOrWhiteSpace(p.PartName) ? partCode : p.PartName!.Trim(),
                    Unit = p.Unit?.Trim() ?? "",
                    Quantity = p.Quantity is > 0 ? p.Quantity!.Value : 0m,
                    Price = p.Price is > 0 ? p.Price!.Value : 0m,
                    Note = p.Note
                });
            }
        }
        await db.SaveChangesAsync();
        return await BuildCampaignViewAsync(camp);
    }

    // Danh sách chiến dịch (Ser_CampaignMarketing_SearchDL): lọc theo từ khóa/trạng thái/cờ hiệu lực.
    public async Task<object> ListCampaignsAsync(string? keyword, string? status, bool? active)
    {
        var q = db.CampaignMarketings.Where(x => x.OrgId == Org);
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            q = q.Where(x => x.CamMarketingNo.Contains(k) || x.CamMarketingName.Contains(k));
        }
        if (!string.IsNullOrWhiteSpace(status))
        {
            var s = status.Trim().ToUpperInvariant();
            q = q.Where(x => x.CamMarketingStatus == s);
        }
        var list = await q.OrderBy(x => x.CamMarketingNo).ToListAsync();
        var today = DateTime.Today;
        var views = new List<object>();
        foreach (var c in list)
        {
            var isActive = CampaignStatuses.IsActive(c.CamMarketingStatus)
                && (!c.EffDateStart.HasValue || c.EffDateStart.Value.Date <= today)
                && (!c.EffDateEnd.HasValue || c.EffDateEnd.Value.Date >= today);
            if (active.HasValue && active.Value != isActive) continue;
            var partCount = await db.CampaignMarketingParts.CountAsync(x => x.OrgId == Org && x.CamMarketingNo == c.CamMarketingNo);
            var condCount = await db.CampaignMarketingConditions.CountAsync(x => x.OrgId == Org && x.CamMarketingNo == c.CamMarketingNo);
            views.Add(new
            {
                c.Id, c.CamMarketingNo, c.CamMarketingName, c.Description, c.CamMarketingStatus,
                c.EffDateStart, c.EffDateEnd, c.WarrantyDateStart, c.WarrantyDateEnd,
                c.ConditionPlateNo, c.ConditionDealer, c.ConditionVIN, c.ConditionFullVIN,
                c.CreatedAt, isActive, partCount, conditionCount = condCount
            });
        }
        return new { count = views.Count, items = views };
    }

    // Chi tiết 1 chiến dịch (kèm điều kiện + phụ tùng).
    public async Task<object?> GetCampaignAsync(string camMarketingNo)
    {
        var no = (camMarketingNo ?? "").Trim();
        var camp = await db.CampaignMarketings.FirstOrDefaultAsync(x => x.OrgId == Org && x.CamMarketingNo == no);
        return camp is null ? null : await BuildCampaignViewAsync(camp);
    }

    // Xóa chiến dịch + điều kiện + phụ tùng.
    public async Task<object?> DeleteCampaignAsync(string camMarketingNo)
    {
        var no = (camMarketingNo ?? "").Trim();
        var camp = await db.CampaignMarketings.FirstOrDefaultAsync(x => x.OrgId == Org && x.CamMarketingNo == no);
        if (camp is null) return null;
        var conds = await db.CampaignMarketingConditions.Where(x => x.OrgId == Org && x.CamMarketingNo == no).ToListAsync();
        var parts = await db.CampaignMarketingParts.Where(x => x.OrgId == Org && x.CamMarketingNo == no).ToListAsync();
        db.CampaignMarketingConditions.RemoveRange(conds);
        db.CampaignMarketingParts.RemoveRange(parts);
        db.CampaignMarketings.Remove(camp);
        await db.SaveChangesAsync();
        return new { camp.CamMarketingNo, removedConditions = conds.Count, removedParts = parts.Count };
    }

    // Ser_CampaignMarketing_GetForRoPartItem: lọc chiến dịch đang hiệu lực áp dụng cho xe/RO.
    // Điều kiện: CamMarketingStatus='A'; khoảng hiệu lực (EffDateStart..EffDateEnd) chứa ngày xét;
    // khoảng ngày kích hoạt bảo hành chứa WarrantyRegistrationDate của xe; và các điều kiện biển số/đại lý/VIN.
    public async Task<object> MatchCampaignsAsync(MatchCampaignsDto dto)
    {
        var effDate = DateTime.TryParse(dto.EffDate, out var ed) ? ed.Date : DateTime.Today;
        var carIds = SplitList(dto.CarIds);
        var roIds = SplitList(dto.RoIds);
        // Xe xét: lấy từ lịch hẹn (Vin/Plate/DealerCode) theo CarIds (ở đây CarId ↔ Appointment.Code) hoặc theo RO.
        var cars = new List<(string CarId, string? Plate, string? FrameNo, string DealerCode, DateTime? WarrantyDate)>();
        if (carIds.Length > 0)
        {
            var appts = await db.Appointments.Where(a => a.OrgId == Org && carIds.Contains(a.Code)).ToListAsync();
            foreach (var a in appts)
                cars.Add((a.Code, a.Plate, a.Vin, a.DealerCode ?? "", null));
        }
        if (roIds.Length > 0)
        {
            var ros = await db.RepairOrders.Where(r => r.OrgId == Org && roIds.Contains(r.RoId)).ToListAsync();
            foreach (var r in ros)
                cars.Add((r.RoId, r.PlateNo, r.FrameNo, r.DealerCode ?? "", null));
        }

        var campaigns = await db.CampaignMarketings.Where(x => x.OrgId == Org && x.CamMarketingStatus == CampaignStatuses.Approve).ToListAsync();
        var conds = await db.CampaignMarketingConditions.Where(x => x.OrgId == Org).ToListAsync();
        var parts = await db.CampaignMarketingParts.Where(x => x.OrgId == Org).ToListAsync();

        var matched = new List<object>();
        foreach (var c in campaigns)
        {
            // Khoảng hiệu lực chiến dịch phải chứa ngày xét (null = không ràng buộc).
            if (c.EffDateStart.HasValue && c.EffDateStart.Value.Date > effDate) continue;
            if (c.EffDateEnd.HasValue && c.EffDateEnd.Value.Date < effDate) continue;

            var cConds = conds.Where(x => x.CamMarketingNo == c.CamMarketingNo).ToList();
            var matchedCars = new List<string>();
            foreach (var car in cars)
            {
                // Điều kiện ngày kích hoạt bảo hành (WarrantyDateStart..WarrantyDateEnd chứa WarrantyRegistrationDate).
                if (c.WarrantyDateStart.HasValue || c.WarrantyDateEnd.HasValue)
                {
                    if (!car.WarrantyDate.HasValue) continue;
                    var wd = car.WarrantyDate.Value.Date;
                    if (c.WarrantyDateStart.HasValue && c.WarrantyDateStart.Value.Date > wd) continue;
                    if (c.WarrantyDateEnd.HasValue && c.WarrantyDateEnd.Value.Date < wd) continue;
                }
                // Điều kiện biển số: khớp tiền tố (StartPlateNo + '%').
                if (c.ConditionPlateNo)
                {
                    var plate = car.Plate ?? "";
                    if (!cConds.Any(x => x.ConditionType == CampaignConditionTypes.PlateNo && plate.StartsWith(x.Value, StringComparison.OrdinalIgnoreCase))) continue;
                }
                // Điều kiện đại lý: khớp bằng.
                if (c.ConditionDealer)
                {
                    if (!cConds.Any(x => x.ConditionType == CampaignConditionTypes.Dealer && string.Equals(x.Value, car.DealerCode, StringComparison.OrdinalIgnoreCase))) continue;
                }
                // Điều kiện ký tự VIN: khớp chứa.
                if (c.ConditionVIN)
                {
                    var frame = car.FrameNo ?? "";
                    if (!cConds.Any(x => x.ConditionType == CampaignConditionTypes.VIN && frame.Contains(x.Value, StringComparison.OrdinalIgnoreCase))) continue;
                }
                // Điều kiện VIN đầy đủ: khớp bằng.
                if (c.ConditionFullVIN)
                {
                    if (!cConds.Any(x => x.ConditionType == CampaignConditionTypes.FullVIN && string.Equals(x.Value, car.FrameNo ?? "", StringComparison.OrdinalIgnoreCase))) continue;
                }
                matchedCars.Add(car.CarId);
            }
            if (cars.Count > 0 && matchedCars.Count == 0) continue;   // có xe xét nhưng không xe nào khớp
            matched.Add(new
            {
                c.CamMarketingNo, c.CamMarketingName, c.Description, c.CamMarketingStatus,
                c.EffDateStart, c.EffDateEnd, c.WarrantyDateStart, c.WarrantyDateEnd,
                matchedCarIds = matchedCars,
                parts = parts.Where(p => p.CamMarketingNo == c.CamMarketingNo)
                    .Select(p => new { p.PartCode, p.PartName, p.Unit, p.Quantity, p.Price, p.Note })
            });
        }
        return new { effDate = effDate.ToString("yyyy-MM-dd"), carCount = cars.Count, count = matched.Count, items = matched };
    }

    // Dựng view 1 chiến dịch: header + điều kiện + phụ tùng + tổng tiền phụ tùng.
    private async Task<object> BuildCampaignViewAsync(CampaignMarketing c)
    {
        var conds = await db.CampaignMarketingConditions.Where(x => x.OrgId == Org && x.CamMarketingNo == c.CamMarketingNo)
            .OrderBy(x => x.ConditionType).ThenBy(x => x.Value).ToListAsync();
        var parts = await db.CampaignMarketingParts.Where(x => x.OrgId == Org && x.CamMarketingNo == c.CamMarketingNo)
            .OrderBy(x => x.PartCode).ToListAsync();
        return new
        {
            c.Id, c.CamMarketingNo, c.CamMarketingName, c.Description, c.CamMarketingStatus,
            c.EffDateStart, c.EffDateEnd, c.WarrantyDateStart, c.WarrantyDateEnd,
            c.ConditionPlateNo, c.ConditionDealer, c.ConditionVIN, c.ConditionFullVIN, c.CreatedAt,
            conditions = conds.Select(x => new { x.ConditionType, x.Value }),
            parts = parts.Select(p => new { p.PartCode, p.PartName, p.Unit, p.Quantity, p.Price, p.Note }),
            partAmount = Math.Round(parts.Sum(p => p.Price * p.Quantity), 2)
        };
    }

    // ===== Thiết lập bảo dưỡng định kỳ (Ser_MST_ROMaintanceSetting) =====
    // Ser_MST_ROMaintanceSetting_Get: danh sách thiết lập bảo dưỡng (lọc theo khoảng Km + cờ hiệu lực + phân trang).
    public async Task<object> ListMaintenanceSettingsAsync(int? minKm, int? maxKm, bool? active, int? recordStart, int? recordCount)
    {
        var q = db.MaintenanceSettings.Where(x => x.OrgId == Org);
        if (minKm.HasValue) q = q.Where(x => x.Km >= minKm.Value);
        if (maxKm.HasValue) q = q.Where(x => x.Km <= maxKm.Value);
        if (active.HasValue) q = q.Where(x => x.FlagActive == active.Value);
        var total = await q.CountAsync();
        var start = Math.Max(0, recordStart ?? 0);
        var count = recordCount is > 0 ? recordCount!.Value : 100;
        var items = await q.OrderBy(x => x.Km).Skip(start).Take(count)
            .Select(x => new { x.Id, x.RomsId, x.Km, x.Maintances, x.FlagActive, x.LogLUDateTime, x.LogLUBy })
            .ToListAsync();
        return new { total, count = items.Count, recordStart = start, items };
    }

    public async Task<object?> GetMaintenanceSettingAsync(string romsId)
    {
        var r = romsId.Trim().ToUpperInvariant();
        var x = await db.MaintenanceSettings.FirstOrDefaultAsync(m => m.OrgId == Org && m.RomsId == r);
        if (x is null) return null;
        return new { x.Id, x.RomsId, x.Km, x.Maintances, x.FlagActive, x.LogLUDateTime, x.LogLUBy };
    }

    // Ser_MST_ROMaintanceSetting_Save: tạo/cập nhật thiết lập bảo dưỡng.
    // Validate: Km là số nguyên dương (Save_KMNotInteger), Maintances >= 0 (Save_MaintancesNotInteger),
    // Km không trùng (Save_KmExisted), ROMSID (nếu có) phải tồn tại (Save_ROMSIDNotFound).
    public async Task<object> SaveMaintenanceSettingAsync(SaveMaintenanceSettingDto dto)
    {
        if (!MaintenanceRules.IsValidKm(dto.Km))
            throw new InvalidOperationException($"Mốc Km '{dto.Km}' không hợp lệ (phải là số nguyên dương).");
        if (!MaintenanceRules.IsValidMaintances(dto.Maintances))
            throw new InvalidOperationException($"Số lần bảo dưỡng '{dto.Maintances}' không hợp lệ (phải >= 0).");

        var romsId = dto.RomsId?.Trim().ToUpperInvariant();
        MaintenanceSetting? x;
        if (!string.IsNullOrWhiteSpace(romsId))
        {
            x = await db.MaintenanceSettings.FirstOrDefaultAsync(m => m.OrgId == Org && m.RomsId == romsId);
            if (x is null) throw new InvalidOperationException($"Mã thiết lập '{romsId}' không tồn tại.");
        }
        else
        {
            romsId = "ROMS" + DateTime.Now.ToString("yyMMddHHmmss") + Random.Shared.Next(10, 99);
            x = new MaintenanceSetting { OrgId = Org, RomsId = romsId };
            db.MaintenanceSettings.Add(x);
        }

        // Km duy nhất (Ser_MST_ROMaintanceSetting_Save_KmExisted).
        var dupKm = await db.MaintenanceSettings.AnyAsync(m => m.OrgId == Org && m.Km == dto.Km && m.Id != x.Id);
        if (dupKm) throw new InvalidOperationException($"Mốc {dto.Km} km đã tồn tại trong hệ thống.");

        x.Km = dto.Km;
        x.Maintances = dto.Maintances;
        if (dto.FlagActive.HasValue) x.FlagActive = dto.FlagActive.Value;
        x.LogLUDateTime = DateTime.Now;
        x.LogLUBy = dto.LogLUBy;
        await db.SaveChangesAsync();
        return new { x.Id, x.RomsId, x.Km, x.Maintances, x.FlagActive, x.LogLUDateTime, x.LogLUBy };
    }

    public async Task<object?> DeleteMaintenanceSettingAsync(string romsId)
    {
        var r = romsId.Trim().ToUpperInvariant();
        var x = await db.MaintenanceSettings.FirstOrDefaultAsync(m => m.OrgId == Org && m.RomsId == r);
        if (x is null) return null;
        db.MaintenanceSettings.Remove(x);
        await db.SaveChangesAsync();
        return new { x.RomsId, deleted = true };
    }

    // Gợi ý mốc bảo dưỡng kế tiếp theo số Km hiện tại: mốc nhỏ nhất có Km >= km hiện tại (còn hiệu lực).
    public async Task<object?> SuggestMaintenanceForKmAsync(int km)
    {
        if (km < 0) km = 0;
        var next = await db.MaintenanceSettings.Where(m => m.OrgId == Org && m.FlagActive && m.Km >= km)
            .OrderBy(m => m.Km).FirstOrDefaultAsync();
        if (next is null) return null;
        return new { currentKm = km, nextKm = next.Km, next.RomsId, next.Maintances, remainingKm = next.Km - km };
    }

    // ===== Thống kê lệnh sửa chữa theo ngày (Ser_RO_Sumary_DL) =====
    // Lọc theo đại lý ('|'), khoảng ngày CheckInDate (FromDate..ToDate) và trạng thái ('|').
    // Doanh thu mỗi RO = Σ phụ tùng (Price*Quantity*Factor*(1+VAT/100)) + Σ công việc (Price*Factor*(1+VAT/100)).
    // Trả kèm tên nhóm trạng thái (Chờ sửa/Đang sửa/Sửa xong/Đã giao xe/Hủy, hẹn lại/Lệnh hủy).
    public async Task<object> SummarizeRepairOrdersAsync(RoSummaryDto dto)
    {
        var dealerCodes = SplitList(dto.DealerCodes);
        var statuses = SplitList(dto.Statuses);
        DateTime? from = DateTime.TryParse(dto.FromDate, out var f) ? f.Date : null;
        DateTime? to = DateTime.TryParse(dto.ToDate, out var t) ? t.Date : null;

        var q = db.RepairOrders.Where(x => x.OrgId == Org);
        if (dealerCodes.Length > 0) q = q.Where(x => dealerCodes.Contains(x.DealerCode));
        if (statuses.Length > 0) q = q.Where(x => statuses.Contains(x.Status));
        if (from.HasValue) q = q.Where(x => x.CheckInDate != null && x.CheckInDate >= from.Value);
        if (to.HasValue) q = q.Where(x => x.CheckInDate != null && x.CheckInDate <= to.Value.AddDays(1).AddTicks(-1));

        var ros = await q.OrderBy(x => x.CheckInDate).ThenBy(x => x.RoId).Take(2000)
            .Select(x => new { x.RoId, x.RoNo, x.PlateNo, x.CusRequest, x.CheckInDate, x.Status, x.DealerCode })
            .ToListAsync();
        var roIds = ros.Select(x => x.RoId).ToList();

        // Tổng tiền công việc (Ser_ROServiceItems) và phụ tùng (Ser_ROPartItems) theo ROID.
        var svcTotals = await db.RepairOrderServiceItems.Where(x => x.OrgId == Org && roIds.Contains(x.RoId))
            .GroupBy(x => x.RoId)
            .Select(g => new { RoId = g.Key, Total = g.Sum(x => x.Price * x.Factor * (1 + x.VAT / 100m)) })
            .ToListAsync();
        var partTotals = await db.RepairOrderPartItems.Where(x => x.OrgId == Org && roIds.Contains(x.RoId))
            .GroupBy(x => x.RoId)
            .Select(g => new { RoId = g.Key, Total = g.Sum(x => x.Price * x.Quantity * x.Factor * (1 + x.VAT / 100m)) })
            .ToListAsync();
        var svcByRo = svcTotals.ToDictionary(x => x.RoId, x => x.Total);
        var partByRo = partTotals.ToDictionary(x => x.RoId, x => x.Total);

        var items = ros.Select(x =>
        {
            var svc = svcByRo.TryGetValue(x.RoId, out var s) ? s : 0m;
            var part = partByRo.TryGetValue(x.RoId, out var p) ? p : 0m;
            return new
            {
                x.RoId, x.RoNo, x.PlateNo, x.CusRequest, x.CheckInDate, x.DealerCode,
                status = x.Status, statusText = RoStages.Text(x.Status), group = RoStages.Group(x.Status),
                serviceAmount = Math.Round(svc, 2), partAmount = Math.Round(part, 2), revenue = Math.Round(svc + part, 2)
            };
        }).ToList();

        return new
        {
            fromDate = from?.ToString("yyyy-MM-dd"), toDate = to?.ToString("yyyy-MM-dd"),
            count = items.Count,
            totalRevenue = Math.Round(items.Sum(x => x.revenue), 2),
            totalServiceAmount = Math.Round(items.Sum(x => x.serviceAmount), 2),
            totalPartAmount = Math.Round(items.Sum(x => x.partAmount), 2),
            byGroup = items.GroupBy(x => x.group).Select(g => new { group = g.Key, count = g.Count(), revenue = Math.Round(g.Sum(x => x.revenue), 2) }),
            items
        };
    }

    // Ser_App_UpdateStatusDL / Ser_App_UpdateStatusX: đổi trạng thái lịch hẹn theo máy trạng thái AppStatus
    // (1=Mới tạo, 2=Xác nhận, 3=Tiếp nhận, 4=Hủy, 5=Đã liên hệ & Chưa xác nhận).
    // Quy tắc nguồn: không cho Hủy (4) khi lịch đã Tiếp nhận (3) — Ser_App_UpdateStatusX_StatusReceptionNotCancel;
    // trạng thái đích phải nằm trong danh sách nguồn hợp lệ (Ser_App_CheckDB_AppStatusNotMatched).
    public async Task<object?> ChangeAppointmentStatusAsync(string code, ChangeApptStatusDto dto)
    {
        var a = await Get(code);
        if (a is null) return null;

        var to = (dto.ToStatus ?? "").Trim();
        if (!ApptStatusRules.All.Contains(to))
            throw new InvalidOperationException($"Trạng thái '{to}' không hợp lệ (1=Mới tạo, 2=Xác nhận, 3=Tiếp nhận, 4=Hủy, 5=Đã liên hệ & Chưa xác nhận).");

        var from = ApptStatusRules.CodeOf(a.Status);
        if (from.Length == 0)
            throw new InvalidOperationException($"Lịch {a.Code} đang ở trạng thái '{a.Status}' không đổi được qua API này.");
        if (from == to)
            throw new InvalidOperationException($"Lịch {a.Code} đã ở trạng thái '{ApptStatusRules.Text(to)}'.");

        // Ser_App_UpdateStatusX_StatusReceptionNotCancel: chặn Hủy khi đã Tiếp nhận.
        if (to == ApptStatusRules.CancelCode && from == ApptStatusRules.ReceptionCode)
            throw new InvalidOperationException($"Lịch {a.Code} đã tiếp nhận, không thể hủy.");

        // Ser_App_CheckDB_AppStatusNotMatched: trạng thái hiện tại phải nằm trong danh sách nguồn hợp lệ.
        if (!ApptStatusRules.CanTransition(from, to))
            throw new InvalidOperationException($"Không thể chuyển lịch {a.Code} từ '{ApptStatusRules.Text(from)}' sang '{ApptStatusRules.Text(to)}'.");

        var now = DateTime.Now;
        a.Status = to switch
        {
            ApptStatusRules.NewStatus     => ApptStatus.Requested,
            ApptStatusRules.ConfirmedCode => ApptStatus.Confirmed,
            ApptStatusRules.ReceptionCode => ApptStatus.CheckedIn,
            ApptStatusRules.CancelCode    => ApptStatus.Cancelled,
            ApptStatusRules.ContactedCode => ApptStatus.Contacted,
            _                             => a.Status
        };
        // Mốc thời gian tương ứng khi vào trạng thái (giống các endpoint chuyên biệt).
        if (a.Status == ApptStatus.CheckedIn) a.CheckedInAt = now;
        if (a.Status == ApptStatus.Contacted) a.ContactedAt = now;

        db.ApptStatusHistories.Add(new ApptStatusHistory
        {
            OrgId = Org, AppCode = a.Code, FromStatus = from, ToStatus = to,
            Note = dto.Note, ChangedBy = dto.ChangedBy, ChangedAt = now
        });
        await db.SaveChangesAsync();
        return new { a.Code, fromStatus = from, fromStatusText = ApptStatusRules.Text(from), toStatus = to, toStatusText = ApptStatusRules.Text(to), status = a.Status.ToString(), statusText = Text(a.Status), changedAt = now };
    }

    // Lịch sử đổi trạng thái lịch hẹn (Ser_App_UpdateStatusDL) — mới nhất trước.
    public async Task<object?> GetAppointmentStatusHistoryAsync(string code)
    {
        var a = await Get(code);
        if (a is null) return null;
        var items = await db.ApptStatusHistories.Where(x => x.OrgId == Org && x.AppCode == a.Code)
            .OrderByDescending(x => x.ChangedAt)
            .Select(x => new { x.Id, x.FromStatus, x.ToStatus, x.Note, x.ChangedBy, x.ChangedAt })
            .ToListAsync();
        return new
        {
            a.Code, status = a.Status.ToString(), statusText = Text(a.Status),
            count = items.Count,
            items = items.Select(x => new
            {
                x.Id, x.FromStatus, fromStatusText = ApptStatusRules.Text(x.FromStatus),
                x.ToStatus, toStatusText = ApptStatusRules.Text(x.ToStatus),
                x.Note, x.ChangedBy, x.ChangedAt
            })
        };
    }

    // ===== Tìm lịch hẹn theo khoảng ngày + cờ trạng thái (Ser_App_GetStatusList01WHDL / SerAppSearchWHDL) =====
    // Bộ lọc: DealerCode (đại lý), khoảng ngày hẹn AppDateTimeFrom..AppDateTimeTo, biển số/tên KH/người tạo (chứa),
    // loại cuộc hẹn (đa giá trị '|'), và 5 cờ trạng thái (Mới tạo/Xác nhận/Đã liên hệ/Tiếp nhận/Hủy) gom thành AppStatusList.
    // Phân trang theo TRANG (Ft_PageIndex 1-based / Ft_PageSize). Trả kèm thông tin xe/KH/KTV/khoang/RO đã join.
    public async Task<object> SearchAppointmentsWhAsync(SearchAppointmentsWhDto dto)
    {
        var q = db.Appointments.Where(a => a.OrgId == Org);

        // DealerCode: đại lý (đơn giá trị).
        if (!string.IsNullOrWhiteSpace(dto.DealerCode))
        {
            var d = dto.DealerCode.Trim();
            q = q.Where(a => a.DealerCode == d);
        }

        // AppDateTimeFrom..AppDateTimeTo: khoảng ngày hẹn (GenDateRangeCondition trên Ser_App.AppDateTimeFrom).
        if (!string.IsNullOrWhiteSpace(dto.AppDateTimeFrom) && DateTime.TryParse(dto.AppDateTimeFrom, out var from))
            q = q.Where(a => a.PreferredAt >= from);
        if (!string.IsNullOrWhiteSpace(dto.AppDateTimeTo) && DateTime.TryParse(dto.AppDateTimeTo, out var to))
            q = q.Where(a => a.PreferredAt <= to);

        // PlateNo: LIKE (chứa) trên biển số.
        if (!string.IsNullOrWhiteSpace(dto.PlateNo))
        {
            var p = dto.PlateNo.Trim();
            q = q.Where(a => a.Plate != null && a.Plate.Contains(p));
        }

        // CusName: LIKE (chứa) trên tên khách hàng.
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

        // 5 cờ trạng thái → AppStatusList (1 Mới tạo / 2 Xác nhận / 5 Đã liên hệ / 3 Tiếp nhận / 4 Hủy).
        // Không bật cờ nào → không lọc theo trạng thái (giống nguồn khi AppStatusList rỗng).
        var statusCodes = new List<string>();
        if (dto.FlagMoiTao == true) statusCodes.Add(ApptStatusRules.NewStatus);
        if (dto.FlagXacNhan == true) statusCodes.Add(ApptStatusRules.ConfirmedCode);
        if (dto.FlagDaLienHe == true) statusCodes.Add(ApptStatusRules.ContactedCode);
        if (dto.FlagTiepNhan == true) statusCodes.Add(ApptStatusRules.ReceptionCode);
        if (dto.FlagHuy == true) statusCodes.Add(ApptStatusRules.CancelCode);
        var statuses = statusCodes.Select(ParseSourceStatus).Where(s => s.HasValue).Select(s => s!.Value).ToArray();
        if (statuses.Length > 0) q = q.Where(a => statuses.Contains(a.Status));

        var total = await q.CountAsync();

        // Phân trang theo trang (Ft_PageIndex 1-based, Ft_PageSize mặc định 20, tối đa 500).
        var pageIndex = Math.Max(1, dto.PageIndex ?? 1);
        var pageSize = Math.Clamp(dto.PageSize ?? 20, 1, 500);
        var rows = await q.OrderBy(a => a.PreferredAt).ThenBy(a => a.Id)
            .Skip((pageIndex - 1) * pageSize).Take(pageSize)
            .Select(a => new
            {
                a.Code, a.CustomerName, a.Phone, a.Vin, a.Plate, a.ServiceType, a.PreferredAt,
                a.DealerCode, a.Engineer, status = a.Status.ToString(), statusText = Text(a.Status),
                a.RoNo, a.RoId, a.BayCode, a.AppTypeCode, a.SlotFrom, a.SlotTo,
                a.ContactedAt, a.ContactResult, a.CreatedAt
            }).ToListAsync();

        // Join thông tin xe/KH/KTV/khoang/RO (Ser_Car/Ser_Customer/Ser_Engineer/Ser_Cavity/Ser_RO).
        var bayCodes = rows.Where(r => !string.IsNullOrWhiteSpace(r.BayCode)).Select(r => r.BayCode!).Distinct().ToList();
        var bays = await db.ServiceBays.Where(b => b.OrgId == Org && bayCodes.Contains(b.Code))
            .Select(b => new { b.Code, b.Name, b.BayType }).ToListAsync();
        var engCodes = rows.Where(r => !string.IsNullOrWhiteSpace(r.Engineer)).Select(r => r.Engineer!).Distinct().ToList();
        var engineers = await db.Engineers.Where(e => e.OrgId == Org && engCodes.Contains(e.Code))
            .Select(e => new { e.Code, e.Name, e.Skill }).ToListAsync();
        var roIds = rows.Where(r => !string.IsNullOrWhiteSpace(r.RoId)).Select(r => r.RoId!).Distinct().ToList();
        var ros = await db.RepairOrders.Where(r => r.OrgId == Org && roIds.Contains(r.RoId))
            .Select(r => new { r.RoId, r.RoNo, r.Status }).ToListAsync();

        var items = rows.Select(r => new
        {
            r.Code, r.CustomerName, r.Phone, r.Vin, r.Plate, r.ServiceType, r.PreferredAt,
            r.DealerCode, r.Engineer, r.status, r.statusText, r.RoNo, r.RoId, r.BayCode, r.AppTypeCode,
            r.SlotFrom, r.SlotTo, r.ContactedAt, r.ContactResult, r.CreatedAt,
            bay = string.IsNullOrWhiteSpace(r.BayCode) ? null : bays.FirstOrDefault(b => b.Code == r.BayCode),
            engineer = string.IsNullOrWhiteSpace(r.Engineer) ? null : engineers.FirstOrDefault(e => e.Code == r.Engineer),
            ro = string.IsNullOrWhiteSpace(r.RoId) ? null : ros.FirstOrDefault(x => x.RoId == r.RoId)
        }).ToList();

        return new
        {
            total, pageIndex, pageSize, count = items.Count,
            totalPages = pageSize >0 ? (int)Math.Ceiling(total / (double)pageSize) : 0,
            statusCodes, items
        };
    }
}
