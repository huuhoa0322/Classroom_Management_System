namespace CLS.BLL.DTOs.Users;

/// <summary>Response khi reset password — trả mật khẩu ngẫu nhiên cho Admin hiển thị 1 lần.</summary>
public class ResetPasswordResponse
{
    public string NewPassword { get; set; } = string.Empty;
}
