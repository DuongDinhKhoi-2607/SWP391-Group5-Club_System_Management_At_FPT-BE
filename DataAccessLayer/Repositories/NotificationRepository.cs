using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly ClubSystemDbContext _context;

    public NotificationRepository(ClubSystemDbContext context)
    {
        _context = context;
    }

    public async Task<Notification> CreateAsync(Notification notification, List<long> recipientUserIds)
    {
        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(); // sinh notificationid

            if (recipientUserIds.Count > 0)
            {
                var recipients = recipientUserIds.Select(uid => new Notificationrecipient
                {
                    Notificationid = notification.Notificationid,
                    Userid         = uid,
                    Isread         = false
                }).ToList();

                _context.Notificationrecipients.AddRange(recipients);
                await _context.SaveChangesAsync();
            }

            await tx.CommitAsync();
            return notification;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<List<Notification>> GetAllAsync()
    {
        return await _context.Notifications
            .Include(n => n.Sender)
            .Include(n => n.Club)
            .Include(n => n.Notificationrecipients)
            .OrderByDescending(n => n.Createdat)
            .ToListAsync();
    }

    public async Task<List<(Notification notification, bool isRead, DateTime? readAt)>> GetByRecipientAsync(long userId)
    {
        var rows = await _context.Notificationrecipients
            .Where(r => r.Userid == userId)
            .Include(r => r.Notification)
            .OrderByDescending(r => r.Notification.Createdat)
            .Select(r => new
            {
                r.Notification,
                r.Isread,
                r.Readat
            })
            .ToListAsync();

        return rows.Select(r => (r.Notification, r.Isread, r.Readat)).ToList();
    }

    public async Task<bool> MarkAsReadAsync(long notificationId, long userId)
    {
        var recipient = await _context.Notificationrecipients
            .FirstOrDefaultAsync(r => r.Notificationid == notificationId && r.Userid == userId);

        if (recipient == null) return false;
        if (recipient.Isread) return true; // đã đọc rồi

        recipient.Isread = true;
        recipient.Readat = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<long>> GetAllUserIdsAsync()
        => await _context.Users.Select(u => u.Userid).ToListAsync();

    public async Task<List<long>> GetUserIdsByRoleAsync(string role)
        => await _context.Users
            .Where(u => u.Systemrole == role)
            .Select(u => u.Userid)
            .ToListAsync();

    public async Task<List<long>> GetActiveClubMemberIdsAsync(long clubId)
        => await _context.Memberships
            .Where(m => m.Clubid == clubId && m.Status == "Đang sinh hoạt")
            .Select(m => m.Userid)
            .ToListAsync();
}
