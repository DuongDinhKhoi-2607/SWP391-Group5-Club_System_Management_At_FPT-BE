using System.Security.Claims;
using BussinessLayer.DTOs;
using BussinessLayer.DTOs.Club;
using BussinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
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

        // ─────────────────────────────────────────────────────────────
        // GET /api/clubs?status=Đang hoạt động
        // ─────────────────────────────────────────────────────────────

        /// <summary>[Public] Danh sách CLB. Truyền ?status=... để filter.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            try
            {
                var clubs = await _service.GetAllAsync(status);
                return Ok(new { message = "Lấy danh sách CLB thành công.", data = clubs });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/clubs/{clubId}
        // ─────────────────────────────────────────────────────────────

        /// <summary>[Public] Chi tiết 1 CLB kèm Leader hiện tại.</summary>
        [HttpGet("{clubId:long}")]
        public async Task<IActionResult> GetById(long clubId)
        {
            try
            {
                var club = await _service.GetByIdAsync(clubId);
                if (club == null)
                    return NotFound(new { message = $"Không tìm thấy CLB với ID {clubId}." });

                return Ok(new { message = "Lấy thông tin CLB thành công.", data = club });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // POST /api/clubs
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// [ADMIN] Tạo CLB mới và thiết lập Leader.
        /// Quy tắc với LeaderStudentId (MSSV của Leader):
        ///   ✅ Sinh viên TỒN TẠI trong bảng Student + CHƯA có tài khoản → hệ thống TỰ TẠO User (role=MEMBER, position=Leader).
        ///   ❌ Sinh viên KHÔNG TỒN TẠI → lỗi 400.
        ///   ❌ Sinh viên ĐÃ CÓ tài khoản → lỗi 400 (không tạo được).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateClub([FromBody] CreateClubDto dto)
        {
            try
            {
                var result = await _service.CreateClubAsync(dto);
                return StatusCode(201, new
                {
                    message = "Tạo CLB thành công. Đã tạo tài khoản cho Leader.",
                    data = new
                    {
                        result.Clubid,
                        result.Clubname,
                        result.Clubcode,
                        result.Description,
                        result.Status,
                        result.Totalactivemembers,
                        result.Createdat,
                        leader = new
                        {
                            studentId  = dto.LeaderStudentId,
                            systemRole = "MEMBER",
                            position   = "Leader",
                            note       = "Tài khoản mới được tạo tự động với mật khẩu mặc định là MSSV."
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // PUT /api/clubs/{clubId}
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// [ADMIN / Leader] Cập nhật thông tin CLB.
        /// - ADMIN: bypass kiểm tra Leader, cập nhật bất kỳ CLB nào.
        /// - Leader: chỉ được cập nhật CLB mà mình là Leader hiện tại.
        /// </summary>
        [HttpPut("{clubId:long}")]
        [Authorize]
        public async Task<IActionResult> UpdateClub(long clubId, [FromBody] UpdateClubDto dto)
        {
            try
            {
                var role = User.FindFirstValue(ClaimTypes.Role) ?? "";

                if (role.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
                {
                    var result = await _service.UpdateClubByAdminAsync(clubId, dto);
                    return Ok(new { message = "Cập nhật thông tin CLB thành công.", data = result });
                }
                else
                {
                    // NameClaimType = "sub" → ClaimTypes.NameIdentifier giờ map đúng
                    var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    if (!long.TryParse(userIdStr, out long currentUserId))
                        return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu userId." });

                    var result = await _service.UpdateClubAsync(clubId, dto, currentUserId);
                    return Ok(new { message = "Cập nhật thông tin CLB thành công.", data = result });
                }
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

        // ─────────────────────────────────────────────────────────────
        // PATCH /api/clubs/{clubId}/status
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// [ADMIN] Đổi trạng thái CLB.
        /// Các giá trị hợp lệ: "Đang hoạt động" | "Tạm dừng" | "Giải thể".
        /// </summary>
        [HttpPatch("{clubId:long}/status")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateStatus(long clubId, [FromBody] UpdateClubStatusDto dto)
        {
            try
            {
                await _service.UpdateStatusAsync(clubId, dto.Status);
                return Ok(new
                {
                    message = $"Đã cập nhật trạng thái CLB {clubId} thành '{dto.Status}'.",
                    clubId,
                    newStatus = dto.Status
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}