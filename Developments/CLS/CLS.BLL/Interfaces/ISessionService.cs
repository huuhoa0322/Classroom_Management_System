using CLS.BLL.Common;
using CLS.BLL.DTOs.Sessions;

namespace CLS.BLL.Interfaces;

/// <summary>
/// Service interface cho Schedule Management (CLS-004 + CLS-005).
/// </summary>
public interface ISessionService
{
    /// <summary>Tạo buổi học mới (AC1: conflict check tự động).</summary>
    Task<ServiceResult<SessionResponse>> CreateSessionAsync(CreateSessionRequest request, CancellationToken ct = default);

    /// <summary>Cập nhật buổi học (conflict check khi đổi teacher/room/time).</summary>
    Task<ServiceResult<SessionResponse>> UpdateSessionAsync(int id, UpdateSessionRequest request, CancellationToken ct = default);

    /// <summary>Xóa buổi học (soft-delete).</summary>
    Task<ServiceResult<object?>> DeleteSessionAsync(int id, CancellationToken ct = default);

    /// <summary>Lấy tất cả sessions (phân trang).</summary>
    Task<PagedResult<SessionResponse>> GetAllSessionsAsync(int page, int pageSize, CancellationToken ct = default);

    // ── Dropdown Data ────────────────────────────────────────────────────
    Task<IEnumerable<ClassDto>> GetClassesAsync(CancellationToken ct = default);
    Task<IEnumerable<RoomDto>> GetRoomsAsync(CancellationToken ct = default);
    Task<IEnumerable<TeacherDto>> GetTeachersAsync(CancellationToken ct = default);
}
