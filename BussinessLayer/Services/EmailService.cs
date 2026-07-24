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
        var sendGridApiKey = _config["EmailSettings:SendGridApiKey"];
        var senderEmail = _config["EmailSettings:SenderEmail"] ?? "se182046duongdinhkhoi@gmail.com";

        if (string.IsNullOrEmpty(sendGridApiKey))
        {
            throw new Exception("SendGrid API Key is missing. Please add it to appsettings.json.");
        }

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", sendGridApiKey);

        var payload = new
        {
            personalizations = new[]
            {
                new { to = new[] { new { email = toEmail } } }
            },
            from = new { email = senderEmail, name = "FPT Club System" },
            subject = subject,
            content = new[]
            {
                new { type = isHtml ? "text/html" : "text/plain", value = body }
            }
        };

        var response = await client.PostAsJsonAsync("https://api.sendgrid.com/v3/mail/send", payload);

        if (!response.IsSuccessStatusCode)
        {
            var errorDetail = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to send email via SendGrid: {response.StatusCode} - {errorDetail}");
        }
    }
}
