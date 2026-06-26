namespace BussinessLayer.DTOs.ClubReport;

// ─── Response: trả về khi GET list hoặc GET by id ───────────────────────────
public class ClubReportResponseDto
{
    public long ClubReportId { get; set; }
    public long ClubId { get; set; }
    public string ClubName { get; set; } = null!;
    public string ClubCode { get; set; } = null!;
    public long ReportPeriodId { get; set; }
    public string ReportPeriodName { get; set; } = null!;
    public string ReportTitle { get; set; } = null!;
    public string? SummaryContent { get; set; }
    public string? IcpdpFeedback { get; set; }
    public long? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public string? ManagerNote { get; set; }
    public DateTime? ManagerReviewedAt { get; set; }
    public int TotalEventsHeld { get; set; }
    public decimal FinancialBalance { get; set; }
    public string Status { get; set; } = null!;
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

// ─── Request: Admin duyệt / từ chối + ghi feedback ──────────────────────────
public class ReviewClubReportRequestDto
{
    /// <summary>
    /// Giá trị hợp lệ: "Đã duyệt" | "Từ chối"
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Nhận xét / phản hồi của Admin gửi lại cho CLB (tuỳ chọn)
    /// </summary>
    public string? IcpdpFeedback { get; set; }
}
