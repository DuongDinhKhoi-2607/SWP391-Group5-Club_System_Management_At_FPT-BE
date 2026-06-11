using BussinessLayer.DTOs.Auth;
using BussinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Bước 1: Đăng nhập.
    /// - Admin → nhận AccessToken ngay.
    /// - 1 CLB → nhận AccessToken ngay.
    /// - Nhiều CLB → nhận TempToken + AvailableClubs, RequireClubSelection = true.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        try
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Bước 2: Chọn CLB (chỉ dùng khi RequireClubSelection = true).
    /// Input: TempToken + ClubId → nhận AccessToken chính thức.
    /// </summary>
    [HttpPost("select-club")]
    [AllowAnonymous]
    public async Task<IActionResult> SelectClub([FromBody] SelectClubRequestDto dto)
    {
        try
        {
            var result = await _authService.SelectClubAsync(dto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Logout stateless — client tự xóa token.
    /// Endpoint này chỉ để client có điểm gọi tường minh.
    /// </summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    public IActionResult Logout()
    {
        return NoContent(); // 204
    }

    /// <summary>
    /// Trả về thông tin user từ JWT claims (cần token hợp lệ).
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var sub       = User.FindFirst("sub")?.Value
                     ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var username  = User.FindFirst("username")?.Value;
        var sysRole   = User.FindFirst("system_role")?.Value;
        var clubId    = User.FindFirst("club_id")?.Value;
        var clubRole  = User.FindFirst("club_role")?.Value;

        return Ok(new
        {
            userId     = sub,
            username,
            systemRole = sysRole,
            clubId,
            clubRole
        });
    }
}
