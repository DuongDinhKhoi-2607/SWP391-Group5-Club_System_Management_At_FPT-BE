using BussinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers;

[Route("api/dashboard")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Lấy thống kê tổng quan hệ thống cho Admin Dashboard.
    /// Yêu cầu: ADMIN
    /// </summary>
    [HttpGet("admin")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetAdminDashboardStats()
    {
        try
        {
            var stats = await _dashboardService.GetDashboardStatsAsync();
            return Ok(new { message = "Lấy thống kê hệ thống thành công.", data = stats });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lấy thống kê cho Manager Dashboard.
    /// Yêu cầu: Manager
    /// </summary>
    [HttpGet("manager")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> GetManagerDashboardStats()
    {
        try
        {
            var stats = await _dashboardService.GetDashboardStatsAsync();
            
            // Lọc ra các số liệu cần thiết cho Manager
            var managerStats = new {
                totalClubs = stats.TotalClubs,
                activeClubs = stats.ActiveClubs,
                pendingEvents = await _dashboardService.GetPendingEventsCountAsync(), // We need to add this to service
                pendingEvidences = await _dashboardService.GetPendingEvidencesCountAsync(), // We need to add this
                pendingReports = stats.PendingReportsForManager
            };
            
            return Ok(new { message = "Lấy thống kê hệ thống thành công.", data = managerStats });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
