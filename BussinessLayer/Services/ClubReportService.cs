using BussinessLayer.DTOs.ClubReport;
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

    public ClubReportService(IClubReportRepository repo)
    {
        _repo = repo;
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

    public async Task<ClubReportResponseDto> ReviewAsync(long clubReportId, ReviewClubReportRequestDto dto)
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
        return MapToDto(report);
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
