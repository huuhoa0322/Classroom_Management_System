namespace CLS.BLL.DTOs.Attendance;

/// <summary>Request DTO: Submit điểm danh toàn bộ buổi học.</summary>
public class SubmitAttendanceRequest
{
    /// <summary>Danh sách bản ghi điểm danh.</summary>
    public List<AttendanceRecord> Records { get; set; } = [];
}

/// <summary>Bản ghi điểm danh 1 học sinh.</summary>
public class AttendanceRecord
{
    public int StudentId { get; set; }

    /// <summary>Trạng thái: present, absent, late.</summary>
    public string Status { get; set; } = "absent";

    /// <summary>Ghi chú tùy chọn.</summary>
    public string? Note { get; set; }
}
