namespace CLS.BLL.DTOs.Classes;

/// <summary>Request DTO đổi trạng thái lớp học (active/inactive).</summary>
public class UpdateClassStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
