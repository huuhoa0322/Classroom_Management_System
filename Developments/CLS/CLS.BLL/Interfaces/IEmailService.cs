namespace CLS.BLL.Interfaces;

/// <summary>
/// Email service abstraction (UC-11).
/// Implement bằng SmtpEmailService (MailKit) hoặc SupabaseEmailService.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Gửi email HTML.
    /// </summary>
    /// <returns>True nếu gửi thành công, False nếu thất bại sau retries.</returns>
    Task<bool> SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
}
