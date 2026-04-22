namespace CLS.BLL.DTOs.Students;

/// <summary>Request DTO để đổi trạng thái vòng đời học sinh (CLS-002 AC1).</summary>
public class UpdateStudentStatusRequest
{
    /// <summary>Trạng thái mới: 'active' hoặc 'inactive'.</summary>
    public string Status { get; set; } = string.Empty;
}
