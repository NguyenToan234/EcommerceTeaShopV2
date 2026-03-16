using EcommerceTeaShop.Common.Settings;
using EcommerceTeaShop.Service.Contract;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        message.Body = new TextPart("html")
        {
            Text = body
        };

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            _settings.SmtpServer,
            _settings.Port,
            SecureSocketOptions.StartTlsWhenAvailable
        );

        await smtp.AuthenticateAsync(
            _settings.Username,
            _settings.Password
        );

        await smtp.SendAsync(message);

        await smtp.DisconnectAsync(true);
    }
}