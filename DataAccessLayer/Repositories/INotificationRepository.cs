using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories;

public interface INotificationRepository
{
    /// <summary>Tạo thông báo + danh sách recipients trong 1 transaction</summary>
    Task<Notification> CreateAsync(Notification notification, List<long> recipientUserIds);

    /// <summary>Admin: lấy tất cả thông báo đã gửi (kèm sender, club, recipient count)</summary>
    Task<List<Notification>> GetAllAsync();

    /// <summary>User: lấy thông báo của mình (qua notificationrecipient)</summary>
    Task<List<(Notification notification, bool isRead, DateTime? readAt)>> GetByRecipientAsync(long userId);

    /// <summary>Đánh dấu đã đọc</summary>
    Task<bool> MarkAsReadAsync(long notificationId, long userId);

    // Helpers để xác định danh sách recipient
    Task<List<long>> GetAllUserIdsAsync();
    Task<List<long>> GetUserIdsByRoleAsync(string role);
    Task<List<long>> GetActiveClubMemberIdsAsync(long clubId);
}
