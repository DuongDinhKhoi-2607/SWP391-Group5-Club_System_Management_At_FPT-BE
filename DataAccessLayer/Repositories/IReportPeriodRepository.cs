using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories;

public interface IReportPeriodRepository
{
    Task<List<Reportperiod>> GetAllAsync(long? semesterId);
    Task<Reportperiod?> GetByIdAsync(long id);
    Task AddAsync(Reportperiod period);
    Task UpdateAsync(Reportperiod period);
    Task<int> CountPendingReportsAsync();
}
