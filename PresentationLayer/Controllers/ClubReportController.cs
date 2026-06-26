using BussinessLayer.DTOs.ClubReport;
using BussinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers;

[Route("api/club-reports")]
[ApiController]
[Authorize(Roles = "ADMIN")]
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
            var reports = await _service.GetAllAsync(reportPeriodId, clubId, status);
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
            var report = await _service.GetByIdAsync(clubReportId);
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
}
