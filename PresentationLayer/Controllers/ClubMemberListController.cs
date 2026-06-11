using BussinessLayer.DTOs;
using BussinessLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [Route("api/clubs")]
    [ApiController]
    public class ClubMemberListController : ControllerBase
    {
        private readonly IClubMemberListService _service;

        public ClubMemberListController(IClubMemberListService service)
        {
            _service = service;
        }

        [HttpGet("{clubId}/members")]
        public async Task<IActionResult> GetActiveMembersByClub(long clubId)
        {
            try
            {
                // Tạm hard-code để test
                // Sau này lấy từ JWT
                long currentUserId = 2;

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
                long currentUserId = 2;

                var result = await _service.AddMemberByStudentIdAsync(dto, currentUserId);

                return Ok(new
                {
                    message = "Thêm thành viên vào CLB thành công.",
                    loginEmail = result.SchoolEmail,
                    defaultPassword = result.StudentId,
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
    }
}