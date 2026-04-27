using CLS.BLL.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace CLS.BLL.Services;

/// <summary>
/// Email service sử dụng MailKit (SMTP) — UC-11.
/// Hỗ trợ Gmail, Outlook, và mọi SMTP provider.
/// Exponential retry: max 3 lần (2s → 4s → 8s).
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly ILogger<SmtpEmailService> _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _username;
    private readonly string _password;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly bool _enableSsl;
    private readonly int _maxRetries;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    {
        _logger     = logger;
        _smtpHost   = config["EmailSettings:Smtp:Host"] ?? "smtp.gmail.com";
        _smtpPort   = config.GetValue("EmailSettings:Smtp:Port", 587);
        _username   = config["EmailSettings:Smtp:Username"] ?? "";
        _password   = config["EmailSettings:Smtp:Password"] ?? "";
        _fromEmail  = config["EmailSettings:Smtp:FromEmail"] ?? _username;
        _fromName   = config["EmailSettings:Smtp:FromName"] ?? "CLS Notification";
        _enableSsl  = config.GetValue("EmailSettings:Smtp:EnableSsl", true);
        _maxRetries = config.GetValue("EmailSettings:MaxRetries", 3);
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _fromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        for (int attempt = 1; attempt <= _maxRetries; attempt++)
        {
            try
            {
                using var client = new SmtpClient();

                var secureOption = _enableSsl
                    ? (_smtpPort == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls)
                    : SecureSocketOptions.None;

                await client.ConnectAsync(_smtpHost, _smtpPort, secureOption, ct);
                await client.AuthenticateAsync(_username, _password, ct);
                await client.SendAsync(message, ct);
                await client.DisconnectAsync(true, ct);

                _logger.LogInformation(
                    "Email sent successfully to {To}, subject: {Subject} (attempt {Attempt})",
                    to, subject, attempt);

                return true;
            }
            catch (Exception ex) when (attempt < _maxRetries && ex is not OperationCanceledException)
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // 2s, 4s, 8s
                _logger.LogWarning(ex,
                    "Email send failed to {To} (attempt {Attempt}/{Max}). Retrying in {Delay}s...",
                    to, attempt, _maxRetries, delay.TotalSeconds);

                await Task.Delay(delay, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Email send failed permanently to {To} after {Attempt} attempts",
                    to, attempt);

                return false;
            }
        }

        return false;
    }
}
