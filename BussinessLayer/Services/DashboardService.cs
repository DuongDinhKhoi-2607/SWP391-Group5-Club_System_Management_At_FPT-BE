using BussinessLayer.DTOs.Dashboard;
using BussinessLayer.Interfaces;
using DataAccessLayer.Repositories;

namespace BussinessLayer.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repo;

    public DashboardService(IDashboardRepository repo)
    {
        _repo = repo;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        // Phải await tuần tự — EF Core DbContext không hỗ trợ concurrent queries
        // trên cùng 1 scoped instance (Task.WhenAll sẽ gây lỗi threading)
        var totalClubs      = await _repo.CountClubsAsync();
        var activeClubs     = await _repo.CountClubsAsync("Đang hoạt động");
        var suspendedClubs  = await _repo.CountClubsAsync("Tạm dừng");
        var totalUsers      = await _repo.CountUsersAsync();
        var activeMembers   = await _repo.CountActiveMembersAsync();
        var totalEvents     = await _repo.CountEventsAsync();
        var upcomingEvents  = await _repo.CountEventsAsync("Đang diễn ra");
        var pendingAdmin    = await _repo.CountPendingReportsForAdminAsync();
        var pendingManager  = await _repo.CountPendingReportsForManagerAsync();

        return new DashboardStatsDto
        {
            TotalClubs               = totalClubs,
            ActiveClubs              = activeClubs,
            SuspendedClubs           = suspendedClubs,
            TotalUsers               = totalUsers,
            TotalActiveMembers       = activeMembers,
            TotalEvents              = totalEvents,
            UpcomingOrOngoingEvents  = upcomingEvents,
            PendingReportsForAdmin   = pendingAdmin,
            PendingReportsForManager = pendingManager,
        };
    }
}
