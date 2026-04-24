namespace CLS.DAL.Entities;

/// <summary>
/// Buổi học — một slot thời gian cụ thể gán Class + Teacher + Room.
/// Bảng: sessions
///
/// Status lifecycle:
///   scheduled → in_progress → completed
///   scheduled / in_progress → cancelled
/// </summary>
public class Session : BaseEntity
{
    /// <summary>FK tới lớp học.</summary>
    public int ClassId { get; set; }

    /// <summary>FK tới giáo viên (User role=Teacher).</summary>
    public int TeacherId { get; set; }

    /// <summary>FK tới phòng học.</summary>
    public int RoomId { get; set; }

    /// <summary>Thời điểm bắt đầu (UTC).</summary>
    public DateTime StartTime { get; set; }

    /// <summary>Thời điểm kết thúc (UTC).</summary>
    public DateTime EndTime { get; set; }

    /// <summary>Trạng thái: scheduled, in_progress, completed, cancelled.</summary>
    public string Status { get; set; } = "scheduled";

    // ── Navigation Properties ────────────────────────────────────────────
    public Class Class { get; set; } = null!;
    public User Teacher { get; set; } = null!;
    public Room Room { get; set; } = null!;
}
