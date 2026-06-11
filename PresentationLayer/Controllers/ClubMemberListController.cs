using BussinessLayer.DTOs;
using BussinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PresentationLayer.Controllers
{
    [Route("api/clubs")]
    [ApiController]
    [Authorize]
    public class ClubMemberListController : ControllerBase
    {
        private readonly IClubMemberListService _service;

        public ClubMemberListController(IClubMemberListService service)
        {
            _service = service;
        }

        private long GetCurrentUserId()
        {
            var userId =
                User.FindFirst("sub")?.Value ??
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("Không tìm thấy userId trong token.");

            return long.Parse(userId);
        }

        [HttpGet("{clubId}/members")]
        public async Task<IActionResult> GetActiveMembersByClub(long clubId)
        {
            try
            {
                long currentUserId = GetCurrentUserId();

                var result = await _service.GetActiveMembersByClubAsync(
                    clubId,
                    currentUserId
                );

                return Ok(new
                {
                    total = result.Count,
                    data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [HttpPost("/api/member/add-member-by-student-id")]
        public async Task<IActionResult> AddMemberByStudentId([FromBody] AddClubMemberDto dto)
        {
            try
            {
                long currentUserId = GetCurrentUserId();

                var result = await _service.AddMemberByStudentIdAsync(
                    dto,
                    currentUserId
                );

                return Ok(new
                {
                    message = "Thêm thành viên vào CLB thành công.",
                    loginEmail = result.SchoolEmail,
                    defaultPassword = result.StudentId,
                    note = "Mật khẩu mặc định là MSSV. Thành viên có thể đổi mật khẩu sau khi đăng nhập.",
                    data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("/api/member/view-member-detail/{membershipId}")]
        [AllowAnonymous]
        public async Task<IActionResult> ViewMemberDetail(long membershipId)
        {
            try
            {
                var result = await _service.GetMemberDetailAsync(membershipId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [HttpPut("/api/member/remove-member/{membershipId}")]
        public async Task<IActionResult> RemoveMember(long membershipId)
        {
            try
            {
                long currentUserId = GetCurrentUserId();

                await _service.RemoveMemberAsync(
                    membershipId,
                    currentUserId
                );

                return Ok(new
                {
                    message = "Xóa thành viên khỏi CLB thành công. Thành viên đã được chuyển sang trạng thái 'Đã rút lui'."
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }
    }
}