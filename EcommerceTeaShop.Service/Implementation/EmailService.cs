using EcommerceTeaShop.Service.Contract;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtp = new SmtpClient(
            _configuration["Email:Host"],
            int.Parse(_configuration["Email:Port"])
        )
        {
            Credentials = new NetworkCredential(
                _configuration["Email:Username"],
                _configuration["Email:Password"]
            ),
            EnableSsl = true,
            UseDefaultCredentials = false,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        var message = new MailMessage
        {
            From = new MailAddress(_configuration["Email:Username"], "TeaVault System"),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        message.To.Add(to);

        await smtp.SendMailAsync(message);
    }
}