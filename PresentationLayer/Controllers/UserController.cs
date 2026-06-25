using BussinessLayer.DTOs.User;
using BussinessLayer.Interfaces;
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
}
