namespace CLS.BLL.DTOs.Attendance;

/// <summary>Response DTO: Sheet điểm danh — session info + danh sách học sinh cần điểm danh.</summary>
public class AttendanceSheetDto
{
    public int SessionId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string SessionStatus { get; set; } = string.Empty;

    /// <summary>Danh sách học sinh active trong lớp.</summary>
    public List<StudentAttendanceItem> Students { get; set; } = [];

    /// <summary>Điểm danh đã ghi nhận (null nếu chưa điểm danh).</summary>
    public List<AttendanceDto>? ExistingRecords { get; set; }
}

/// <summary>Thông tin 1 học sinh trong sheet điểm danh.</summary>
public class StudentAttendanceItem
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
}
