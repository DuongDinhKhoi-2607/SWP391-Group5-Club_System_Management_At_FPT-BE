using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class ClubReportRepository : IClubReportRepository
{
    private readonly ClubSystemDbContext _context;

    public ClubReportRepository(ClubSystemDbContext context)
    {
        _context = context;
    }

    public async Task<List<Clubreport>> GetAllAsync(long? reportPeriodId, long? clubId, string? status)
    {
        var query = _context.Clubreports
            .Include(r => r.Club)
            .Include(r => r.Reportperiod)
            .Include(r => r.Manager).ThenInclude(m => m!.Userinformation!.Student)
            .AsQueryable();

        if (reportPeriodId.HasValue)
            query = query.Where(r => r.Reportperiodid == reportPeriodId.Value);

        if (clubId.HasValue)
            query = query.Where(r => r.Clubid == clubId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.Status == status);

        return await query.OrderByDescending(r => r.Submittedat).ToListAsync();
    }

    public async Task<Clubreport?> GetByIdAsync(long clubReportId)
    {
        return await _context.Clubreports
            .Include(r => r.Club)
            .Include(r => r.Reportperiod)
            .Include(r => r.Manager).ThenInclude(m => m!.Userinformation!.Student)
            .FirstOrDefaultAsync(r => r.Clubreportid == clubReportId);
    }

    public async Task<List<Clubreport>> GetAllForAdminAsync(
        long? reportPeriodId, long? clubId, string? status, HashSet<string> allowedStatuses)
    {
        var query = _context.Clubreports
            .Include(r => r.Club)
            .Include(r => r.Reportperiod)
            .Include(r => r.Manager).ThenInclude(m => m!.Userinformation!.Student)
            .AsQueryable();

        // Chỉ lấy các status Admin được phép xem
        query = query.Where(r => allowedStatuses.Contains(r.Status));

        if (reportPeriodId.HasValue)
            query = query.Where(r => r.Reportperiodid == reportPeriodId.Value);

        if (clubId.HasValue)
            query = query.Where(r => r.Clubid == clubId.Value);

        // Admin filter thêm theo status cụ thể (VD: chỉ xem "Chờ Admin duyệt")
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.Status == status);

        return await query.OrderByDescending(r => r.Submittedat).ToListAsync();
    }

    public async Task UpdateAsync(Clubreport report)
    {
        _context.Clubreports.Update(report);
        await _context.SaveChangesAsync();
    }

    public async Task AddAsync(Clubreport report)
    {
        _context.Clubreports.Add(report);
        await _context.SaveChangesAsync();
    }
}
