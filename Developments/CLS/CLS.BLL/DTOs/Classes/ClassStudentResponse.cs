namespace CLS.BLL.DTOs.Classes;

/// <summary>Response DTO cho thành viên lớp học.</summary>
public class ClassStudentResponse
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public DateOnly EnrollmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
