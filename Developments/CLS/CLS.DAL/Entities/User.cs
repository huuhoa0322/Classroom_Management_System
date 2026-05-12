namespace CLS.DAL.Entities;

/// <summary>
/// Entity lưu trữ thông tin tài khoản nội bộ (Admin, Teacher).
/// Bảng: users
/// </summary>
public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    
    /// <summary>Vai trò trong hệ thống: Admin, Teacher (từ AppRoles)</summary>
    public string Role { get; set; } = string.Empty;
    
    /// <summary>Mật khẩu đã được mã hóa bằng BCrypt</summary>
    public string PasswordHash { get; set; } = string.Empty;
    
    public string Status { get; set; } = "active";

    // ── Account Lockout (Security §M3) ────────────────────────────────────────
    /// <summary>Số lần đăng nhập thất bại liên tiếp.</summary>
    public int FailedLoginCount { get; set; } = 0;

    /// <summary>Thời điểm hết lockout (UTC). Null = không bị lock.</summary>
    public DateTime? LockoutEnd { get; set; }

    /// <summary>Admin khóa thủ công — tách biệt với lockout tự động.</summary>
    public bool IsLocked { get; set; } = false;
}
