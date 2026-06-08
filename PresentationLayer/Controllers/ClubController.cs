using BussinessLayer.DTOs;
using BussinessLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [Route("api/clubs")]
    [ApiController]
    public class ClubController : ControllerBase
    {
        private readonly IClubService _service;

        public ClubController(IClubService service)
        {
            _service = service;
        }

        /// <summary>[Admin] Tạo CLB mới và gán Manager từ bảng student</summary>
        [HttpPost]
        public async Task<IActionResult> CreateClub([FromBody] CreateClubDto dto)
        {
            try
            {
                var result = await _service.CreateClubAsync(dto);
                return Ok(new
                {
                    message = "Tạo CLB thành công.",
                    data = new
                    {
                        result.Clubid, result.Clubname, result.Clubcode,
                        result.Description, result.Status, result.Totalactivemembers, result.Createdat,
                        manager = new { studentId = dto.ManagerStudentId, systemRole = "Manager", position = "Leader" }
                    }
                });
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPut("{clubId}")]
        public async Task<IActionResult> UpdateClub(
            long clubId,
            [FromBody] UpdateClubDto dto)
        {
            try
            {
                // Tạm hard-code để test
                // Sau này đổi thành lấy từ JWT
                long currentUserId = 2;

                var result = await _service.UpdateClubAsync(
                    clubId,
                    dto,
                    currentUserId
                );

                return Ok(new
                {
                    message = "Cập nhật thông tin câu lạc bộ thành công.",
                    data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}