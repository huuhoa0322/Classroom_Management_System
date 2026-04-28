namespace CLS.BLL.DTOs.Classes;

/// <summary>
/// Request DTO đăng ký nhiều học sinh vào lớp cùng lúc.
/// </summary>
public class EnrollStudentsRequest
{
    /// <summary>Danh sách ID học sinh cần đăng ký vào lớp.</summary>
    public List<int> StudentIds { get; set; } = [];
}
