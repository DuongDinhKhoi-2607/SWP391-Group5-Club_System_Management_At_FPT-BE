using BussinessLayer.DTOs.Notification;
using BussinessLayer.Interfaces;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories;

namespace BussinessLayer.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;

    public NotificationService(INotificationRepository repo)
    {
        _repo = repo;
    }

    public async Task<NotificationDto> SendNotificationAsync(long senderId, CreateNotificationDto dto)
    {
        // 1. Xác định danh sách recipient
        List<long> recipientIds = dto.TargetType switch
        {
            "Toàn hệ thống" => await _repo.GetAllUserIdsAsync(),

            "Theo role" => string.IsNullOrWhiteSpace(dto.TargetRole)
                ? throw new Exception("TargetRole là bắt buộc khi TargetType = 'Theo role'.")
                : await _repo.GetUserIdsByRoleAsync(dto.TargetRole),

            "Theo CLB" => dto.ClubId == null
                ? throw new Exception("ClubId là bắt buộc khi TargetType = 'Theo CLB'.")
                : await _repo.GetActiveClubMemberIdsAsync(dto.ClubId.Value),

            "Cá nhân" => dto.TargetUserIds == null || dto.TargetUserIds.Count == 0
                ? throw new Exception("TargetUserIds là bắt buộc khi TargetType = 'Cá nhân'.")
                : dto.TargetUserIds.Where(x => x.HasValue).Select(x => (long)x!).ToList(),

            _ => throw new Exception("TargetType không hợp lệ. Chọn: 'Toàn hệ thống' | 'Theo role' | 'Theo CLB' | 'Cá nhân'.")
        };

        if (recipientIds.Count == 0)
            throw new Exception("Không tìm thấy người nhận phù hợp với điều kiện đã chọn.");

        // 2. Tạo entity
        var notification = new Notification
        {
            Senderid         = senderId,
            Title            = dto.Title,
            Content          = dto.Content,
            Notificationtype = dto.NotificationType,
            Targettype       = dto.TargetType,
            Targetrole       = dto.TargetRole,
            Clubid           = dto.ClubId,
            Eventid          = dto.EventId,
            Clubreportid     = dto.ClubReportId,
            Reportperiodid   = dto.ReportPeriodId,
            Createdat        = DateTime.Now
        };

        var created = await _repo.CreateAsync(notification, recipientIds);

        return new NotificationDto
        {
            NotificationId   = created.Notificationid,
            SenderId         = created.Senderid,
            SenderUsername   = string.Empty, // không cần reload sender ở đây
            Title            = created.Title,
            Content          = created.Content,
            NotificationType = created.Notificationtype,
            TargetType       = created.Targettype,
            TargetRole       = created.Targetrole,
            ClubId           = created.Clubid,
            RecipientCount   = recipientIds.Count,
            ReadCount        = 0,
            CreatedAt        = created.Createdat
        };
    }

    public async Task<List<NotificationDto>> GetAllNotificationsAsync()
    {
        var list = await _repo.GetAllAsync();
        return list.Select(n => new NotificationDto
        {
            NotificationId   = n.Notificationid,
            SenderId         = n.Senderid,
            SenderUsername   = n.Sender?.Username ?? string.Empty,
            Title            = n.Title,
            Content          = n.Content,
            NotificationType = n.Notificationtype,
            TargetType       = n.Targettype,
            TargetRole       = n.Targetrole,
            ClubId           = n.Clubid,
            ClubName         = n.Club?.Clubname,
            RecipientCount   = n.Notificationrecipients.Count,
            ReadCount        = n.Notificationrecipients.Count(r => r.Isread),
            CreatedAt        = n.Createdat
        }).ToList();
    }

    public async Task<List<MyNotificationDto>> GetMyNotificationsAsync(long userId)
    {
        var rows = await _repo.GetByRecipientAsync(userId);
        return rows.Select(row => new MyNotificationDto
        {
            NotificationId   = row.notification.Notificationid,
            Title            = row.notification.Title,
            Content          = row.notification.Content,
            NotificationType = row.notification.Notificationtype,
            TargetType       = row.notification.Targettype,
            IsRead           = row.isRead,
            ReadAt           = row.readAt,
            CreatedAt        = row.notification.Createdat
        }).ToList();
    }

    public async Task<bool> MarkAsReadAsync(long notificationId, long userId)
        => await _repo.MarkAsReadAsync(notificationId, userId);
}
