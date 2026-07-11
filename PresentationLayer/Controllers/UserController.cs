using BussinessLayer.DTOs.User;
using BussinessLayer.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers;

[Route("api/users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Lấy danh sách toàn bộ Users trong hệ thống kèm thông tin chi tiết (FullName, Avatar...).
    /// Yêu cầu: ADMIN
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(new { message = "Lấy danh sách người dùng thành công.", data = users });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một người dùng theo ID.
    /// Yêu cầu: ADMIN
    /// </summary>
    [HttpGet("{userId:long}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetUserById(long userId)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(userId);
            return Ok(new { message = "Lấy thông tin người dùng thành công.", data = user });
        }
        catch (Exception ex) when (ex.Message.Contains("Không tìm thấy"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Tạo tài khoản dành riêng cho Ban quản trị (ADMIN hoặc MANAGER).
    /// Yêu cầu: ADMIN
    /// </summary>
    [HttpPost("staff")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CreateStaffUser([FromBody] CreateStaffUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var user = await _userService.CreateStaffUserAsync(dto);
            return Ok(new { message = "Tạo tài khoản quản trị thành công.", data = user });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Chặn / Khóa một người dùng.
    /// Yêu cầu: ADMIN
    /// </summary>
    [HttpPut("{userId:long}/block")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> BlockUser(long userId)
    {
        try
        {
            await _userService.BlockUserAsync(userId);
            return Ok(new { message = "Khóa tài khoản thành công." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Mở khóa một người dùng.
    /// Yêu cầu: ADMIN
    /// </summary>
    [HttpPut("{userId:long}/unblock")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UnblockUser(long userId)
    {
        try
        {
            await _userService.UnblockUserAsync(userId);
            return Ok(new { message = "Mở khóa tài khoản thành công." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// [MEMBER / ALL] Người dùng tự cập nhật hồ sơ cá nhân.
    /// Nhận dữ liệu dưới dạng multipart/form-data để hỗ trợ tải ảnh đại diện lên Cloudinary.
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserProfileDto dto)
    {
        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out long userId))
                return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu userId." });

            var result = await _userService.UpdateProfileAsync(userId, dto);
            return Ok(new { message = "Cập nhật hồ sơ cá nhân thành công.", data = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// [MEMBER / ADMIN] Xem lịch sử hoạt động (CLB, chức vụ, sự kiện) của một thành viên.
    /// Quyền hạn: Chính chủ tự xem hoặc Admin xem bất kỳ ai.
    /// </summary>
    [HttpGet("{userId:long}/activity-history")]
    [Authorize]
    public async Task<IActionResult> GetActivityHistory(long userId)
    {
        try
        {
            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? "";

            if (!long.TryParse(currentUserIdStr, out long currentUserId))
                return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu userId." });

            // Nếu không phải chính chủ và cũng không phải ADMIN -> từ chối
            if (currentUserId != userId && !currentUserRole.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(403, new { message = "Bạn không có quyền xem lịch sử hoạt động của người dùng này." });
            }

            var result = await _userService.GetMemberActivityHistoryAsync(userId);
            return Ok(new { message = "Lấy lịch sử hoạt động thành công.", data = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
