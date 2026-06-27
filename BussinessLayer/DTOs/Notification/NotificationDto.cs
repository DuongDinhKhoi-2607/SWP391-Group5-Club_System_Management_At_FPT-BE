namespace BussinessLayer.DTOs.Notification;

/// <summary>Dùng cho Admin xem lịch sử thông báo đã gửi</summary>
public class NotificationDto
{
    public long NotificationId { get; set; }
    public long SenderId { get; set; }
    public string SenderUsername { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string NotificationType { get; set; } = null!;
    public string TargetType { get; set; } = null!;
    public string? TargetRole { get; set; }
    public long? ClubId { get; set; }
    public string? ClubName { get; set; }
    public int RecipientCount { get; set; }
    public int ReadCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Dùng cho user xem thông báo của mình</summary>
public class MyNotificationDto
{
    public long NotificationId { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string NotificationType { get; set; } = null!;
    public string TargetType { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
