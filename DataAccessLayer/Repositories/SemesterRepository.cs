using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class SemesterRepository : ISemesterRepository
{
    private readonly ClubSystemDbContext _context;

    public SemesterRepository(ClubSystemDbContext context)
    {
        _context = context;
    }

    public async Task<List<Semester>> GetAllAsync()
    {
        return await _context.Semesters
            .OrderByDescending(s => s.Startdate)
            .ToListAsync();
    }

    public async Task<Semester?> GetByIdAsync(long id)
    {
        return await _context.Semesters
            .FirstOrDefaultAsync(s => s.Semesterid == id);
    }

    public async Task AddAsync(Semester semester)
    {
        await _context.Semesters.AddAsync(semester);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Semester semester)
    {
        _context.Semesters.Update(semester);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasOverlappingSemesterAsync(DateOnly startDate, DateOnly endDate, long? excludeSemesterId = null)
    {
        var query = _context.Semesters.AsQueryable();

        if (excludeSemesterId.HasValue)
        {
            query = query.Where(s => s.Semesterid != excludeSemesterId.Value);
        }

        // Overlap logic: A overlaps B if A.Start < B.End AND A.End > B.Start
        return await query.AnyAsync(s => startDate <= s.Enddate && endDate >= s.Startdate);
    }
}
