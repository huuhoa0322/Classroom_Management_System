namespace CLS.BLL.DTOs.Students;

/// <summary>
/// Request DTO để cập nhật thông tin học sinh (CLS-002).
/// Bao gồm cả thông tin phụ huynh — đồng nhất với form tạo mới.
/// Status lifecycle dùng endpoint riêng.
/// </summary>
public class UpdateStudentRequest
{
    // Học sinh
    public string FullName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }

    // Phụ huynh
    public string ParentFullName { get; set; } = string.Empty;
    public string ParentEmail { get; set; } = string.Empty;
    public string? ParentPhone { get; set; }
    public string ParentRelationship { get; set; } = string.Empty;
}
