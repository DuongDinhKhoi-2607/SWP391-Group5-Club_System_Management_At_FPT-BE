using System.Security.Claims;
using BussinessLayer.DTOs.Notification;
using BussinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers;

[Route("api/notifications")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// ASP.NET Core JWT middleware map "sub" → ClaimTypes.NameIdentifier.
    /// Helper này thử cả 2 key để đảm bảo tương thích.
    /// </summary>
    private long? GetCurrentUserId()
    {
        var raw = User.FindFirst("sub")?.Value
               ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(raw, out var id) ? id : null;
    }

    // ──────────────────────────────────────────────
    //  ADMIN endpoints
    // ──────────────────────────────────────────────

    /// <summary>
    /// Admin, Manager gửi thông báo toàn hệ thống.
    /// Leader gửi thông báo cho CLB của mình.
    /// Yêu cầu: Đã đăng nhập (kiểm tra Role và ClubRole bên trong)
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SendNotification([FromBody] CreateNotificationDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var senderId = GetCurrentUserId();
            if (senderId == null)
                return Unauthorized(new { message = "Không xác định được người gửi từ token." });

            var role = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirst("system_role")?.Value ?? "";
            
            // Nếu không phải ADMIN hoặc Manager, phải là Leader
            if (!role.Equals("ADMIN", StringComparison.OrdinalIgnoreCase) && 
                !role.Equals("MANAGER", StringComparison.OrdinalIgnoreCase))
            {
                var clubRole = User.FindFirst("club_role")?.Value;
                if (clubRole != "Leader")
                {
                    return StatusCode(403, new { message = "Chỉ Admin, Manager hoặc Leader mới có quyền gửi thông báo." });
                }

                var clubIdClaim = User.FindFirst("club_id")?.Value;
                if (string.IsNullOrEmpty(clubIdClaim) || !long.TryParse(clubIdClaim, out long currentClubId))
                {
                    return StatusCode(403, new { message = "Không xác định được câu lạc bộ của bạn." });
                }

                // Kiểm tra logic gửi của Leader
                if (dto.TargetType != "Theo CLB" && dto.TargetType != "Cá nhân")
                {
                    return StatusCode(403, new { message = "Leader chỉ có thể gửi thông báo 'Theo CLB' hoặc 'Cá nhân'." });
                }
                
                if (dto.TargetType == "Theo CLB")
                {
                    // Nếu user có truyền ClubId lên nhưng khác với ClubId của họ thì báo lỗi
                    if (dto.ClubId.HasValue && dto.ClubId.Value != currentClubId)
                    {
                        return StatusCode(403, new { message = "Bạn chỉ có thể gửi thông báo cho câu lạc bộ của mình." });
                    }
                    // Nếu không truyền hoặc truyền đúng, gán/ghi đè luôn bằng currentClubId để an toàn
                    dto.ClubId = currentClubId;
                }
            }

            var result = await _notificationService.SendNotificationAsync(senderId.Value, dto);
            return Ok(new { message = $"Đã gửi thông báo đến {result.RecipientCount} người nhận.", data = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Admin và Manager xem lịch sử tất cả thông báo đã gửi.
    /// Yêu cầu: ADMIN, Manager
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "ADMIN,Manager")]
    public async Task<IActionResult> GetAllNotifications()
    {
        try
        {
            var list = await _notificationService.GetAllNotificationsAsync();
            return Ok(new { message = "Lấy danh sách thông báo thành công.", data = list });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ──────────────────────────────────────────────
    //  User endpoints (tất cả role đã đăng nhập)
    // ──────────────────────────────────────────────

    /// <summary>
    /// Lấy danh sách thông báo của người dùng hiện tại.
    /// Yêu cầu: Đã đăng nhập
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyNotifications()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Không xác định được người dùng từ token." });

            var list = await _notificationService.GetMyNotificationsAsync(userId.Value);
            return Ok(new { message = "Lấy thông báo thành công.", data = list });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Đánh dấu thông báo là đã đọc.
    /// Yêu cầu: Đã đăng nhập
    /// </summary>
    [HttpPatch("{notificationId:long}/read")]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(long notificationId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Không xác định được người dùng từ token." });

            var success = await _notificationService.MarkAsReadAsync(notificationId, userId.Value);
            if (!success)
                return NotFound(new { message = "Không tìm thấy thông báo hoặc bạn không phải người nhận." });

            return Ok(new { message = "Đã đánh dấu là đã đọc." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
