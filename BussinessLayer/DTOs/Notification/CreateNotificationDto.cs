using System.ComponentModel.DataAnnotations;

namespace BussinessLayer.DTOs.Notification;

public class CreateNotificationDto
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    [Required]
    public string Content { get; set; } = null!;

    /// <summary>
    /// 'Thông báo chung' | 'Báo cáo' | 'Sự kiện' | 'Evidence' | 'Hệ thống'
    /// </summary>
    public string NotificationType { get; set; } = "Thông báo chung";

    /// <summary>
    /// 'Toàn hệ thống' | 'Theo role' | 'Theo CLB' | 'Cá nhân'
    /// </summary>
    [Required]
    public string TargetType { get; set; } = null!;

    /// <summary>Bắt buộc khi TargetType = 'Theo role'. Giá trị: 'ADMIN' | 'Manager' | 'Member'</summary>
    public string? TargetRole { get; set; }

    /// <summary>Bắt buộc khi TargetType = 'Theo CLB'</summary>
    public long? ClubId { get; set; }

    /// <summary>Bắt buộc khi TargetType = 'Cá nhân' — danh sách userId nhận thông báo</summary>
    public List<long?>? TargetUserIds { get; set; }

    // Optional context links
    public long? EventId { get; set; }
    public long? ClubReportId { get; set; }
    public long? ReportPeriodId { get; set; }
}
