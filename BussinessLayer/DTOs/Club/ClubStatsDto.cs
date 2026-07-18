namespace BussinessLayer.DTOs.Club;

/// <summary>
/// Thống kê chi tiết hoạt động của một CLB.
/// Dùng cho Manager / Admin xem overview từng CLB.
/// </summary>
public class ClubStatsDto
{
    public long ClubId { get; set; }
    public string ClubName { get; set; } = null!;
    public string ClubCode { get; set; } = null!;
    public string Status { get; set; } = null!;

    // ── Thành viên ──
    public int TotalMembers { get; set; }
    public int ActiveMembers { get; set; }

    // ── Sự kiện ──
    public int TotalEvents { get; set; }
    public int PendingEvents { get; set; }
    public int ApprovedEvents { get; set; }
    public int CompletedEvents { get; set; }

    // ── Báo cáo ──
    public int TotalReports { get; set; }
    public int PendingReports { get; set; }
    public int ApprovedReports { get; set; }

    // ── Evidence ──
    public int TotalEvidences { get; set; }
    public int PendingEvidences { get; set; }
}
