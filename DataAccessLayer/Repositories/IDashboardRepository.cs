namespace DataAccessLayer.Repositories;

public interface IDashboardRepository
{
    Task<int> CountClubsAsync(string? status = null);
    Task<int> CountUsersAsync();
    Task<int> CountActiveMembersAsync();
    Task<int> CountEventsAsync(string? status = null);
    Task<int> CountPendingReportsForAdminAsync();
    Task<int> CountPendingReportsForManagerAsync();
}
