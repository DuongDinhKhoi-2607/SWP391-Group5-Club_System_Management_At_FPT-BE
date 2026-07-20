using BussinessLayer.DTOs.ClubReport;
using BussinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PresentationLayer.Controllers;

[Route("api/club-reports")]
[ApiController]
[Authorize]
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
    [Authorize(Roles = "ADMIN,Manager")]
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
    [Authorize(Roles = "ADMIN,Manager")]
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
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;
            if (!long.TryParse(userIdStr, out long adminId))
                return Unauthorized(new { message = "Token không hợp lệ." });

            var result = await _service.ReviewAsync(clubReportId, dto, adminId);
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
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;
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

    // ──────────────────────────────────────────────
    //  LEADER endpoints
    // ──────────────────────────────────────────────
    private bool TryGetLeaderContext(out long leaderId, out long clubId, out IActionResult errorResult)
    {
        leaderId = 0;
        clubId = 0;
        errorResult = null!;

        var role = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirst("system_role")?.Value ?? "";
        if (role.Equals("ADMIN", StringComparison.OrdinalIgnoreCase) || role.Equals("MANAGER", StringComparison.OrdinalIgnoreCase))
        {
            errorResult = StatusCode(403, new { message = "Role này không được phép nộp/xem báo cáo như Leader." });
            return false;
        }

        var clubRole = User.FindFirst("club_role")?.Value;
        if (clubRole != "Leader")
        {
            errorResult = StatusCode(403, new { message = "Chỉ Leader của câu lạc bộ mới được thực hiện hành động này." });
            return false;
        }

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;
        if (!long.TryParse(userIdStr, out leaderId))
        {
            errorResult = Unauthorized(new { message = "Không lấy được ID người dùng từ token." });
            return false;
        }

        var clubIdClaim = User.FindFirst("club_id")?.Value;
        if (string.IsNullOrEmpty(clubIdClaim) || !long.TryParse(clubIdClaim, out clubId))
        {
            errorResult = StatusCode(403, new { message = "Không xác định được câu lạc bộ của bạn." });
            return false;
        }

        return true;
    }

    [HttpPost]
    public async Task<IActionResult> SubmitReport([FromBody] SubmitClubReportRequestDto dto)
    {
        if (!TryGetLeaderContext(out long leaderId, out long clubId, out var errorResult))
            return errorResult;

        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _service.SubmitReportAsync(clubId, dto, leaderId);
            return Ok(new { message = "Nộp báo cáo thành công.", data = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{clubReportId:long}")]
    public async Task<IActionResult> UpdateReport(long clubReportId, [FromBody] UpdateClubReportRequestDto dto)
    {
        if (!TryGetLeaderContext(out long leaderId, out long clubId, out var errorResult))
            return errorResult;

        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _service.UpdateReportAsync(clubReportId, clubId, dto, leaderId);
            return Ok(new { message = "Cập nhật báo cáo thành công.", data = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("my-club")]
    public async Task<IActionResult> GetMyClubReports([FromQuery] long? reportPeriodId)
    {
        if (!TryGetLeaderContext(out long leaderId, out long clubId, out var errorResult))
            return errorResult;

        try
        {
            var list = await _service.GetMyClubReportsAsync(clubId, reportPeriodId);
            return Ok(new { message = "Lấy lịch sử báo cáo thành công.", data = list });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
