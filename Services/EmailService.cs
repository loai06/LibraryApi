using LibraryManagementSystem.Settings;
using Microsoft.Extensions.Options;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Identity.Client;


namespace LibraryManagementSystem.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _settings;

    public EmailService(IOptions<SmtpSettings> settings )
    {
        _settings = settings.Value;
    }

    public async Task SendEmailAsync(
    string to, string subject, string htmlBody)
{
    var message = new MimeMessage();
    message.From.Add(
        new MailboxAddress(_settings.FromName, _settings.FromEmail));

    message.To.Add(MailboxAddress.Parse(to));
    message.Subject = subject;
    message.Body = new TextPart("html") { Text = htmlBody };

    using var client = new SmtpClient();

    await client.ConnectAsync(
        _settings.Host, _settings.Port, SecureSocketOptions.StartTls);

    await client.AuthenticateAsync(
        _settings.Username, _settings.Password);

    await client.SendAsync(message);
    await client.DisconnectAsync(true);
}

    
}