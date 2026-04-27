using CLS.BLL.Common;
using CLS.BLL.DTOs.Attendance;
using CLS.BLL.DTOs.Sessions;

namespace CLS.BLL.Interfaces;

/// <summary>
/// Service interface cho Academic Operations (UC-07 + UC-08).
/// </summary>
public interface IAttendanceService
{
    /// <summary>UC-07: Lấy lịch dạy của Teacher theo tuần.</summary>
    Task<IEnumerable<SessionResponse>> GetTimetableAsync(
        int teacherId, DateTime from, DateTime to, CancellationToken ct = default);

    /// <summary>UC-08: Lấy sheet điểm danh (session info + danh sách học sinh).</summary>
    Task<ServiceResult<AttendanceSheetDto>> GetAttendanceSheetAsync(
        int sessionId, int teacherId, CancellationToken ct = default);

    /// <summary>UC-08: Submit điểm danh cho buổi học.</summary>
    Task<ServiceResult<object?>> SubmitAttendanceAsync(
        int sessionId, int teacherId, SubmitAttendanceRequest request, CancellationToken ct = default);
}
