namespace BussinessLayer.DTOs.Dashboard;

public class DashboardStatsDto
{
    /// <summary>Tổng số CLB trong hệ thống</summary>
    public int TotalClubs { get; set; }

    /// <summary>CLB đang hoạt động</summary>
    public int ActiveClubs { get; set; }

    /// <summary>CLB tạm dừng</summary>
    public int SuspendedClubs { get; set; }

    /// <summary>Tổng số user (tất cả role)</summary>
    public int TotalUsers { get; set; }

    /// <summary>Thành viên đang sinh hoạt (membership status = 'Đang sinh hoạt')</summary>
    public int TotalActiveMembers { get; set; }

    /// <summary>Tổng số sự kiện</summary>
    public int TotalEvents { get; set; }

    /// <summary>Sự kiện đang diễn ra hoặc sắp diễn ra</summary>
    public int UpcomingOrOngoingEvents { get; set; }

    /// <summary>Báo cáo CLB đang chờ Admin duyệt (đã qua Manager)</summary>
    public int PendingReportsForAdmin { get; set; }

    /// <summary>Báo cáo CLB đang chờ Manager duyệt</summary>
    public int PendingReportsForManager { get; set; }
}
