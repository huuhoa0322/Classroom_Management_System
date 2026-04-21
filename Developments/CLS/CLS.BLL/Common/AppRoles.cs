namespace CLS.BLL.Common;

/// <summary>
/// Role constants cho CLS. Dùng trong [Authorize] attribute để tránh magic strings.
///
/// Ví dụ:
///   [Authorize(Roles = AppRoles.Admin)]
///   [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Teacher}")]
/// </summary>
public static class AppRoles
{
    /// <summary>Quản trị viên — toàn quyền hệ thống.</summary>
    public const string Admin = "Admin";

    /// <summary>Giáo viên — quản lý class session, điểm danh.</summary>
    public const string Teacher = "Teacher";

    /// <summary>Phụ huynh — xem lịch học, điểm danh của con em.</summary>
    public const string Parent = "Parent";
}
