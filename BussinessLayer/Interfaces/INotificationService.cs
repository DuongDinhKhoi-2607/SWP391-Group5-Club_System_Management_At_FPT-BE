using BussinessLayer.DTOs.Notification;

namespace BussinessLayer.Interfaces;

public interface INotificationService
{
    /// <summary>Admin gửi thông báo</summary>
    Task<NotificationDto> SendNotificationAsync(long senderId, CreateNotificationDto dto);

    /// <summary>Admin xem lịch sử thông báo đã gửi</summary>
    Task<List<NotificationDto>> GetAllNotificationsAsync();

    /// <summary>User xem thông báo của mình</summary>
    Task<List<MyNotificationDto>> GetMyNotificationsAsync(long userId);

    /// <summary>Đánh dấu đã đọc</summary>
    Task<bool> MarkAsReadAsync(long notificationId, long userId);
}
