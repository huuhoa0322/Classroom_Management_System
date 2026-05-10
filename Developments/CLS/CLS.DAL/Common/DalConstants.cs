namespace CLS.DAL.Common;

/// <summary>
/// Hằng số dùng trong DAL layer — mirror từ AppConstants (BLL) để tránh circular dependency.
/// Khi thêm/sửa status ở AppConstants, phải đồng bộ sang đây.
/// </summary>
public static class DalConstants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Teacher = "Teacher";
    }

    public static class Status
    {
        public const string Active = "active";
        public const string Inactive = "inactive";
    }
}
