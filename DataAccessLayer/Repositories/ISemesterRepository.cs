using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories;

public interface ISemesterRepository
{
    Task<List<Semester>> GetAllAsync();
    Task<Semester?> GetByIdAsync(long id);
    Task AddAsync(Semester semester);
    Task UpdateAsync(Semester semester);
    Task<bool> HasOverlappingSemesterAsync(DateOnly startDate, DateOnly endDate, long? excludeSemesterId = null);
}
