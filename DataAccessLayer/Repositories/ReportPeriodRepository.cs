using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class ReportPeriodRepository : IReportPeriodRepository
{
    private readonly ClubSystemDbContext _context;

    public ReportPeriodRepository(ClubSystemDbContext context)
    {
        _context = context;
    }

    public async Task<List<Reportperiod>> GetAllAsync(long? semesterId)
    {
        var query = _context.Reportperiods.Include(r => r.Semester).AsQueryable();

        if (semesterId.HasValue)
        {
            query = query.Where(r => r.Semesterid == semesterId.Value);
        }

        return await query.OrderByDescending(r => r.Deadline).ToListAsync();
    }

    public async Task<Reportperiod?> GetByIdAsync(long id)
    {
        return await _context.Reportperiods
            .Include(r => r.Semester)
            .FirstOrDefaultAsync(r => r.Reportperiodid == id);
    }

    public async Task AddAsync(Reportperiod period)
    {
        await _context.Reportperiods.AddAsync(period);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Reportperiod period)
    {
        _context.Reportperiods.Update(period);
        await _context.SaveChangesAsync();
    }
}
