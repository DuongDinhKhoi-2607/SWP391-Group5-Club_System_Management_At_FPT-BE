using BussinessLayer.DTOs.ClubReport;
using BussinessLayer.DTOs.Notification;
using BussinessLayer.Interfaces;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories;

namespace BussinessLayer.Services;

public class ClubReportService : IClubReportService
{
    private static readonly HashSet<string> ValidReviewStatuses =
        new(StringComparer.OrdinalIgnoreCase) { "Đã duyệt", "Từ chối" };

    // Các status Admin được phép thấy (chỉ sau khi Manager đã xử lý)
    private static readonly HashSet<string> AdminVisibleStatuses =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Chờ Admin duyệt",
            "Đã duyệt",
            "Từ chối"
        };

    private static readonly HashSet<string> ManagerVisibleStatuses =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Chờ Manager duyệt",
            "Chờ Admin duyệt",
            "Đã duyệt",
            "Từ chối"
        };

    private readonly IClubReportRepository _repo;
    private readonly INotificationService _notificationService;

    public ClubReportService(IClubReportRepository repo, INotificationService notificationService)
    {
        _repo = repo;
        _notificationService = notificationService;
    }

    public async Task<List<ClubReportResponseDto>> GetAllAsync(long? reportPeriodId, long? clubId, string? status, string role = "ADMIN")
    {
        var allowedStatuses = role == "Manager" ? ManagerVisibleStatuses : AdminVisibleStatuses;

        if (!string.IsNullOrWhiteSpace(status) && !allowedStatuses.Contains(status))
            throw new Exception($"Role {role} không có quyền xem báo cáo ở trạng thái '{status}'. " +
                                $"Chỉ xem được: {string.Join(", ", allowedStatuses)}");

        // Nếu không filter status → lấy tất cả status Admin được thấy
        var effectiveStatus = string.IsNullOrWhiteSpace(status) ? null : status;
        var reports = await _repo.GetAllForAdminAsync(reportPeriodId, clubId, effectiveStatus, allowedStatuses);
        return reports.Select(MapToDto).ToList();
    }

    public async Task<ClubReportResponseDto> GetByIdAsync(long clubReportId, string role = "ADMIN")
    {
        var allowedStatuses = role == "Manager" ? ManagerVisibleStatuses : AdminVisibleStatuses;

        var report = await _repo.GetByIdAsync(clubReportId)
            ?? throw new Exception($"Không tìm thấy báo cáo với ID {clubReportId}.");

        if (!allowedStatuses.Contains(report.Status))
            throw new Exception($"Role {role} không có quyền xem báo cáo này (Trạng thái: {report.Status}).");

        return MapToDto(report);
    }

    public async Task<ClubReportResponseDto> ReviewAsync(long clubReportId, ReviewClubReportRequestDto dto, long adminId)
    {
        if (!ValidReviewStatuses.Contains(dto.Status))
            throw new Exception($"Trạng thái không hợp lệ. Chỉ chấp nhận: \"Đã duyệt\" hoặc \"Từ chối\".");

        var report = await _repo.GetByIdAsync(clubReportId)
            ?? throw new Exception($"Không tìm thấy báo cáo với ID {clubReportId}.");

        if (report.Status != "Chờ Admin duyệt")
            throw new Exception($"Báo cáo chưa được Manager xác nhận (trạng thái hiện tại: {report.Status}).");

        report.Status = dto.Status;
        report.IcpdpFeedback = dto.IcpdpFeedback;
        report.Reviewedat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

        await _repo.UpdateAsync(report);

        // Notify Club Leader
        var notiDto = new CreateNotificationDto
        {
            Title = $"Báo cáo đã được Admin chốt: {dto.Status}",
            Content = $"Báo cáo '{report.Reporttitle}' của CLB bạn đã được Admin {dto.Status}. " + (string.IsNullOrEmpty(dto.IcpdpFeedback) ? "" : $"Feedback: {dto.IcpdpFeedback}"),
            NotificationType = "Báo cáo",
            TargetType = "Theo CLB",
            ClubId = report.Clubid,
            ClubReportId = report.Clubreportid,
            ReportPeriodId = report.Reportperiodid
        };
        await _notificationService.SendNotificationAsync(adminId, notiDto);

        return MapToDto(report);
    }

    public async Task<ClubReportResponseDto> ManagerReviewAsync(long clubReportId, ManagerReviewClubReportRequestDto dto, long managerId)
    {
        if (dto.Status != "Chờ Admin duyệt" && dto.Status != "Từ chối")
            throw new Exception("Trạng thái không hợp lệ. Chỉ chấp nhận: \"Chờ Admin duyệt\" hoặc \"Từ chối\".");

        var report = await _repo.GetByIdAsync(clubReportId)
            ?? throw new Exception($"Không tìm thấy báo cáo với ID {clubReportId}.");

        if (report.Status != "Chờ Manager duyệt")
            throw new Exception($"Báo cáo chưa thể duyệt vì đang ở trạng thái: {report.Status}.");

        report.Status = dto.Status;
        report.Managernote = dto.ManagerNote;
        report.Managerid = managerId;
        report.Managerreviewedat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

        await _repo.UpdateAsync(report);

        // Send notifications
        if (dto.Status == "Từ chối")
        {
            var notiDto = new CreateNotificationDto
            {
                Title = "Báo cáo bị Manager từ chối",
                Content = $"Báo cáo '{report.Reporttitle}' của CLB bạn đã bị Manager từ chối. Ghi chú: {dto.ManagerNote}",
                NotificationType = "Báo cáo",
                TargetType = "Theo CLB",
                ClubId = report.Clubid,
                ClubReportId = report.Clubreportid,
                ReportPeriodId = report.Reportperiodid
            };
            await _notificationService.SendNotificationAsync(managerId, notiDto);
        }
        else if (dto.Status == "Chờ Admin duyệt")
        {
            var notiDto = new CreateNotificationDto
            {
                Title = "Báo cáo cần Admin chốt",
                Content = $"Manager vừa duyệt báo cáo '{report.Reporttitle}' của CLB {report.Club?.Clubname}. Vui lòng vào chốt.",
                NotificationType = "Báo cáo",
                TargetType = "Theo role",
                TargetRole = "ADMIN",
                ClubReportId = report.Clubreportid,
                ReportPeriodId = report.Reportperiodid
            };
            await _notificationService.SendNotificationAsync(managerId, notiDto);
        }

        return MapToDto(report);
    }



    public async Task<ClubReportResponseDto> SubmitReportAsync(long clubId, SubmitClubReportRequestDto dto, long leaderId)
    {
        var report = new Clubreport
        {
            Clubid = clubId,
            Reportperiodid = dto.ReportPeriodId,
            Reporttitle = dto.ReportTitle,
            Summarycontent = dto.SummaryContent,
            Totaleventsheld = dto.TotalEventsHeld,
            Financialbalance = dto.FinancialBalance,
            Status = "Chờ Manager duyệt",
            Submittedat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
        };

        await _repo.AddAsync(report);

        var notiDto = new CreateNotificationDto
        {
            Title = "Báo cáo CLB mới nộp",
            Content = $"Một CLB vừa nộp báo cáo '{dto.ReportTitle}'. Vui lòng vào duyệt sơ bộ.",
            NotificationType = "Báo cáo",
            TargetType = "Theo role",
            TargetRole = "Manager",
            ClubId = clubId,
            ClubReportId = report.Clubreportid,
            ReportPeriodId = dto.ReportPeriodId
        };
        await _notificationService.SendNotificationAsync(leaderId, notiDto);

        var created = await _repo.GetByIdAsync(report.Clubreportid);
        return MapToDto(created ?? report);
    }

    public async Task<ClubReportResponseDto> UpdateReportAsync(long clubReportId, long clubId, UpdateClubReportRequestDto dto, long leaderId)
    {
        var report = await _repo.GetByIdAsync(clubReportId)
            ?? throw new Exception("Không tìm thấy báo cáo.");

        if (report.Clubid != clubId)
            throw new Exception("Bạn không có quyền sửa báo cáo của câu lạc bộ khác.");

        if (report.Status != "Từ chối" && report.Status != "Chờ Manager duyệt")
            throw new Exception("Chỉ được sửa báo cáo khi bị từ chối hoặc đang chờ duyệt.");

        report.Reporttitle = dto.ReportTitle;
        report.Summarycontent = dto.SummaryContent;
        report.Totaleventsheld = dto.TotalEventsHeld;
        report.Financialbalance = dto.FinancialBalance;
        report.Status = "Chờ Manager duyệt"; 
        report.Submittedat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

        await _repo.UpdateAsync(report);

        var notiDto = new CreateNotificationDto
        {
            Title = "Báo cáo CLB đã được nộp lại",
            Content = $"Một CLB vừa nộp lại báo cáo '{dto.ReportTitle}'. Vui lòng vào duyệt sơ bộ.",
            NotificationType = "Báo cáo",
            TargetType = "Theo role",
            TargetRole = "Manager",
            ClubId = clubId,
            ClubReportId = report.Clubreportid,
            ReportPeriodId = report.Reportperiodid
        };
        await _notificationService.SendNotificationAsync(leaderId, notiDto);

        var updated = await _repo.GetByIdAsync(report.Clubreportid);
        return MapToDto(updated ?? report);
    }

    public async Task<List<ClubReportResponseDto>> GetMyClubReportsAsync(long clubId, long? reportPeriodId)
    {
        var reports = await _repo.GetAllAsync(reportPeriodId, clubId, null);
        return reports.Select(MapToDto).ToList();
    }

    // ─── Mapper ─────────────────────────────────────────────────────────────
    private static ClubReportResponseDto MapToDto(Clubreport r) => new()
    {
        ClubReportId     = r.Clubreportid,
        ClubId           = r.Clubid,
        ClubName         = r.Club?.Clubname ?? string.Empty,
        ClubCode         = r.Club?.Clubcode ?? string.Empty,
        ReportPeriodId   = r.Reportperiodid,
        ReportPeriodName = r.Reportperiod?.Periodname ?? string.Empty,
        ReportTitle      = r.Reporttitle,
        SummaryContent   = r.Summarycontent,
        IcpdpFeedback    = r.IcpdpFeedback,
        ManagerId        = r.Managerid,
        ManagerName      = r.Manager?.Userinformation?.Student?.Fullname ?? r.Manager?.Username,
        ManagerNote      = r.Managernote,
        ManagerReviewedAt = r.Managerreviewedat,
        TotalEventsHeld  = r.Totaleventsheld,
        FinancialBalance = r.Financialbalance,
        Status           = r.Status,
        SubmittedAt      = r.Submittedat,
        ReviewedAt       = r.Reviewedat
    };
}
