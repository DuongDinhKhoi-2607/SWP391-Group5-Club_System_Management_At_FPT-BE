using BussinessLayer.DTOs.Dashboard;

namespace BussinessLayer.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
    Task<int> GetPendingEventsCountAsync();
    Task<int> GetPendingEvidencesCountAsync();
}
