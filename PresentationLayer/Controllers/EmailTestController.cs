using System;
using System.Threading.Tasks;
using BussinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers;

[Route("api/email")]
[ApiController]
public class EmailTestController : ControllerBase
{
    private readonly IEmailService _emailService;

    public EmailTestController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    /// <summary>
    /// Gửi email thử nghiệm để kiểm tra cấu hình SMTP.
    /// Không yêu cầu đăng nhập để tiện kiểm thử.
    /// </summary>
    [HttpPost("send-test")]
    [AllowAnonymous]
    public async Task<IActionResult> SendTestEmail([FromQuery] string toEmail, [FromQuery] string subject, [FromQuery] string body)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            return BadRequest(new { message = "Email người nhận (toEmail) không được để trống." });

        try
        {
            var defaultSubject = string.IsNullOrWhiteSpace(subject) ? "Thư thử nghiệm từ FPT Club System" : subject;
            var defaultBody = string.IsNullOrWhiteSpace(body) 
                ? "<h3>Chào bạn,</h3><p>Đây là thư thử nghiệm được gửi từ hệ thống quản lý câu lạc bộ FPT Club System để kiểm tra cấu hình SMTP.</p>" 
                : body;

            await _emailService.SendEmailAsync(toEmail, defaultSubject, defaultBody, isHtml: true);
            return Ok(new { message = $"Đã gửi email thử nghiệm thành công tới {toEmail}." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Gửi email thất bại.", error = ex.Message, details = ex.ToString() });
        }
    }
}
