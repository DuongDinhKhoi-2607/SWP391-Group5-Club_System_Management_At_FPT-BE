using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using BussinessLayer.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BussinessLayer.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
    {
        var smtpServer = _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
        var smtpPortStr = _config["EmailSettings:SmtpPort"] ?? "587";
        var senderEmail = _config["EmailSettings:SenderEmail"] ?? "se182046duongdinhkhoi@gmail.com";
        var senderPassword = _config["EmailSettings:SenderPassword"] ?? "vqgjxcpzqbsmtvbf";
        var enableSslStr = _config["EmailSettings:EnableSsl"] ?? "true";

        if (!int.TryParse(smtpPortStr, out int smtpPort))
        {
            smtpPort = 587;
        }

        if (!bool.TryParse(enableSslStr, out bool enableSsl))
        {
            enableSsl = true;
        }

        using var client = new SmtpClient(smtpServer, smtpPort)
        {
            Credentials = new NetworkCredential(senderEmail, senderPassword),
            EnableSsl = enableSsl
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail, "FPT Club System"),
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };

        mailMessage.To.Add(toEmail);

        await client.SendMailAsync(mailMessage);
    }
}
