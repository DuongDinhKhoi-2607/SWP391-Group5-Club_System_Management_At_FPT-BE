using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly ClubSystemDbContext _context;

    public DashboardRepository(ClubSystemDbContext context)
    {
        _context = context;
    }

    /// <summary>Đếm CLB theo status (null = tất cả)</summary>
    public async Task<int> CountClubsAsync(string? status = null)
    {
        var query = _context.Clubs.AsQueryable();
        if (status != null)
            query = query.Where(c => c.Status == status);
        return await query.CountAsync();
    }

    /// <summary>Tổng số tài khoản user</summary>
    public async Task<int> CountUsersAsync()
        => await _context.Users.CountAsync();

    /// <summary>Thành viên đang sinh hoạt trong CLB</summary>
    public async Task<int> CountActiveMembersAsync()
        => await _context.Memberships
            .Where(m => m.Status == "Đang sinh hoạt")
            .CountAsync();

    /// <summary>Đếm sự kiện theo status (null = tất cả)</summary>
    public async Task<int> CountEventsAsync(string? status = null)
    {
        var query = _context.Events.AsQueryable();
        if (status != null)
            query = query.Where(e => e.Status == status);
        return await query.CountAsync();
    }

    /// <summary>Báo cáo chờ Admin duyệt (đã qua Manager — status = 'Chờ Admin duyệt')</summary>
    public async Task<int> CountPendingReportsForAdminAsync()
        => await _context.Clubreports
            .Where(r => r.Status == "Chờ Admin duyệt")
            .CountAsync();

    /// <summary>Báo cáo chờ Manager duyệt (status = 'Chờ Manager duyệt')</summary>
    public async Task<int> CountPendingReportsForManagerAsync()
        => await _context.Clubreports
            .Where(r => r.Status == "Chờ Manager duyệt")
            .CountAsync();

    /// <summary>Evidence đang chờ kiểm tra</summary>
    public async Task<int> CountPendingEvidencesAsync()
        => await _context.Evidences
            .Where(e => e.Isverified == "Đang chờ")
            .CountAsync();
}
