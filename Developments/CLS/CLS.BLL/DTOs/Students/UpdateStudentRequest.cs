namespace CLS.BLL.DTOs.Students;

/// <summary>
/// Request DTO để cập nhật thông tin học sinh (CLS-002).
/// Chỉ cho phép sửa thông tin cá nhân — status lifecycle dùng endpoint riêng.
/// </summary>
public class UpdateStudentRequest
{
    public string FullName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
}
