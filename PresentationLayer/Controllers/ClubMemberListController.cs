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

        private long GetCurrentClubId()
        {
            var clubIdStr = User.FindFirst("club_id")?.Value;
            if (string.IsNullOrWhiteSpace(clubIdStr))
                throw new UnauthorizedAccessException("Không tìm thấy clubId trong token. Vui lòng chọn câu lạc bộ trước.");

            return long.Parse(clubIdStr);
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

        [HttpGet("{clubId}/alumni")]
        public async Task<IActionResult> GetAlumniMembersByClub(long clubId, [FromQuery] string? search)
        {
            try
            {
                long currentUserId = GetCurrentUserId();

                var result = await _service.GetAlumniMembersByClubAsync(
                    clubId,
                    currentUserId,
                    search
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
                long currentClubId = GetCurrentClubId();

                var result = await _service.AddMemberByStudentIdAsync(
                    dto,
                    currentClubId,
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

        /// <summary>
        /// Kích hoạt tài khoản và xác nhận gia nhập Câu lạc bộ thông qua token được gửi từ email.
        /// </summary>
        [HttpGet("/api/member/confirm-activation")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmActivation([FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return BadRequest("Token không được để trống.");

                await _service.ActivateMemberAsync(token);

                // Trả về HTML giao diện thân thiện thông báo thành công
                var htmlContent = @"
                    <!DOCTYPE html>
                    <html lang='vi'>
                    <head>
                        <meta charset='UTF-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <title>Kích hoạt thành công</title>
                        <style>
                            body {
                                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                                background-color: #f4f7f6;
                                display: flex;
                                justify-content: center;
                                align-items: center;
                                height: 100vh;
                                margin: 0;
                            }
                            .card {
                                background: white;
                                padding: 40px;
                                border-radius: 8px;
                                box-shadow: 0 4px 15px rgba(0,0,0,0.1);
                                text-align: center;
                                max-width: 450px;
                            }
                            h1 {
                                color: #4CAF50;
                                margin-bottom: 20px;
                            }
                            p {
                                color: #666;
                                font-size: 16px;
                                line-height: 1.6;
                            }
                            .icon {
                                font-size: 60px;
                                color: #4CAF50;
                                margin-bottom: 20px;
                            }
                            .footer {
                                margin-top: 30px;
                                font-size: 14px;
                                color: #999;
                            }
                        </style>
                    </head>
                    <body>
                        <div class='card'>
                            <div class='icon'>✓</div>
                            <h1>Xác nhận thành công!</h1>
                            <p>Tài khoản của bạn đã được kích hoạt và tư cách thành viên Câu lạc bộ đã chính thức hoạt động.</p>
                            <p>Chúng tôi đã gửi một email chứa thông tin tài khoản và mật khẩu đăng nhập của bạn (hoặc xác nhận tham gia nếu bạn đã có tài khoản). Vui lòng kiểm tra lại hộp thư.</p>
                            <div class='footer'>Hệ thống quản lý câu lạc bộ</div>
                        </div>
                    </body>
                    </html>";

                return Content(htmlContent, "text/html", System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                var errorHtml = $@"
                    <!DOCTYPE html>
                    <html lang='vi'>
                    <head>
                        <meta charset='UTF-8'>
                        <title>Kích hoạt thất bại</title>
                        <style>
                            body {{ font-family: 'Segoe UI', sans-serif; background-color: #f4f7f6; display: flex; justify-content: center; align-items: center; height: 100vh; margin: 0; }}
                            .card {{ background: white; padding: 40px; border-radius: 8px; box-shadow: 0 4px 15px rgba(0,0,0,0.1); text-align: center; max-width: 450px; }}
                            h1 {{ color: #f44336; margin-bottom: 20px; }}
                            p {{ color: #666; font-size: 16px; line-height: 1.6; }}
                            .icon {{ font-size: 60px; color: #f44336; margin-bottom: 20px; }}
                        </style>
                    </head>
                    <body>
                        <div class='card'>
                            <div class='icon'>✗</div>
                            <h1>Kích hoạt thất bại</h1>
                            <p>{ex.Message}</p>
                        </div>
                    </body>
                    </html>";
                return Content(errorHtml, "text/html", System.Text.Encoding.UTF8);
            }
        }
    }
}