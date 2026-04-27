
namespace CLS.BLL.Services;

/// <summary>
/// Static helper render HTML email templates — UC-11.
/// </summary>
public static class EmailTemplateService
{
    /// <summary>
    /// Render HTML template cho Renewal Alert notification.
    /// </summary>
    public static string RenderRenewalAlert(
        string parentName,
        string studentName,
        string packageName,
        int remainingSessions,
        string message)
    {
        // Sanitize inputs to prevent XSS in email clients
        parentName       = System.Net.WebUtility.HtmlEncode(parentName);
        studentName      = System.Net.WebUtility.HtmlEncode(studentName);
        packageName      = System.Net.WebUtility.HtmlEncode(packageName);
        message          = System.Net.WebUtility.HtmlEncode(message);

        return
            """
            <!DOCTYPE html>
            <html lang="vi">
            <head>
                <meta charset="UTF-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <style>
                    body { font-family: 'Segoe UI', Arial, sans-serif; background-color: #f4f6f9; margin: 0; padding: 0; }
                    .container { max-width: 600px; margin: 20px auto; background: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 2px 12px rgba(0,0,0,0.08); }
                    .header { background: linear-gradient(135deg, #4f46e5, #7c3aed); color: white; padding: 24px 32px; }
                    .header h1 { margin: 0; font-size: 20px; font-weight: 600; }
                    .header p { margin: 4px 0 0; opacity: 0.85; font-size: 14px; }
                    .body { padding: 32px; }
                    .alert-box { background: #fef3c7; border-left: 4px solid #f59e0b; padding: 16px; border-radius: 8px; margin-bottom: 20px; }
                    .alert-box strong { color: #92400e; }
                    .info-table { width: 100%; border-collapse: collapse; margin: 16px 0; }
                    .info-table td { padding: 10px 12px; border-bottom: 1px solid #e5e7eb; font-size: 14px; }
                    .info-table td:first-child { color: #6b7280; width: 40%; }
                    .info-table td:last-child { color: #111827; font-weight: 500; }
                    .cta { text-align: center; margin: 24px 0; }
                    .cta a { display: inline-block; background: #4f46e5; color: white; padding: 12px 28px; border-radius: 8px; text-decoration: none; font-weight: 600; font-size: 14px; }
                    .footer { background: #f9fafb; padding: 16px 32px; text-align: center; color: #9ca3af; font-size: 12px; }
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <h1>📚 CLS - Classroom Management System</h1>
                        <p>Thông báo gia hạn gói học</p>
                    </div>
                    <div class="body">
                        <p>Kính gửi <strong>
            """
            + parentName +
            """
                        </strong>,</p>
                        <div class="alert-box">
                            <strong>⚠️ Cảnh báo:</strong>
            """
            + " " + message +
            """

                        </div>
                        <table class="info-table">
                            <tr>
                                <td>Học sinh</td>
                                <td>
            """
            + studentName +
            """
                                </td>
                            </tr>
                            <tr>
                                <td>Gói học</td>
                                <td>
            """
            + packageName +
            """
                                </td>
                            </tr>
                            <tr>
                                <td>Số buổi còn lại</td>
                                <td><strong>
            """
            + remainingSessions +
            """
             buổi</strong></td>
                            </tr>
                        </table>
                        <p>Vui lòng liên hệ trung tâm để gia hạn gói học, đảm bảo việc học tập của con không bị gián đoạn.</p>
                        <div class="cta">
                            <a href="#">Liên hệ gia hạn ngay</a>
                        </div>
                    </div>
                    <div class="footer">
                        <p>Email này được gửi tự động từ hệ thống CLS. Vui lòng không trả lời.</p>
                        <p>© 2026 CLS - Classroom Management System</p>
                    </div>
                </div>
            </body>
            </html>
            """;
    }
}
