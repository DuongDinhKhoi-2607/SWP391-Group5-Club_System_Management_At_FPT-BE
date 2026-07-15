using BussinessLayer.DTOs.ClubReport;
using BussinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PresentationLayer.Controllers;

[Route("api/club-reports")]
[ApiController]
[Authorize(Roles = "ADMIN,Manager")]
public class ClubReportController : ControllerBase
{
    private readonly IClubReportService _service;

    public ClubReportController(IClubReportService service)
    {
        _service = service;
    }

    /// <summary>
    /// [ADMIN] Lấy danh sách báo cáo CLB.
    /// Filter tuỳ chọn: ?reportPeriodId=1 | ?clubId=2 | ?status=Chờ duyệt
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] long? reportPeriodId,
        [FromQuery] long? clubId,
        [FromQuery] string? status)
    {
        try
        {
            var role = User.IsInRole("Manager") ? "Manager" : "ADMIN";
            var reports = await _service.GetAllAsync(reportPeriodId, clubId, status, role);
            return Ok(new { message = "Lấy danh sách báo cáo thành công.", data = reports });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// [ADMIN] Xem chi tiết 1 báo cáo CLB.
    /// </summary>
    [HttpGet("{clubReportId:long}")]
    public async Task<IActionResult> GetById(long clubReportId)
    {
        try
        {
            var role = User.IsInRole("Manager") ? "Manager" : "ADMIN";
            var report = await _service.GetByIdAsync(clubReportId, role);
            return Ok(new { message = "Lấy thông tin báo cáo thành công.", data = report });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// [ADMIN] Duyệt hoặc từ chối một báo cáo CLB.
    /// Body: { "status": "Đã duyệt" | "Từ chối", "icpdpFeedback": "..." }
    /// Chỉ có thể review khi báo cáo đang ở trạng thái "Chờ duyệt".
    /// </summary>
    [HttpPatch("{clubReportId:long}/review")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Review(long clubReportId, [FromBody] ReviewClubReportRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _service.ReviewAsync(clubReportId, dto);
            return Ok(new
            {
                message = $"Báo cáo đã được cập nhật thành '{result.Status}'.",
                data    = result
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// [Manager] Duyệt (chuyển cho Admin) hoặc từ chối báo cáo.
    /// Body: { "status": "Chờ Admin duyệt" | "Từ chối", "managerNote": "..." }
    /// </summary>
    [HttpPatch("{clubReportId:long}/manager-review")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> ManagerReview(long clubReportId, [FromBody] ManagerReviewClubReportRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userIdStr = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out long managerId))
                return Unauthorized(new { message = "Token không hợp lệ." });

            var result = await _service.ManagerReviewAsync(clubReportId, dto, managerId);
            return Ok(new
            {
                message = $"Báo cáo đã được xử lý thành '{result.Status}'.",
                data = result
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
