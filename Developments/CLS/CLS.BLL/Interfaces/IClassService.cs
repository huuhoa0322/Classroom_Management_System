using CLS.BLL.Common;
using CLS.BLL.DTOs.Classes;

namespace CLS.BLL.Interfaces;

/// <summary>
/// Contract cho Class Management — CRUD lớp học + đăng ký học sinh.
/// </summary>
public interface IClassService
{
    Task<PagedResult<ClassResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<ServiceResult<ClassResponse>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<ClassResponse>> CreateAsync(CreateClassRequest request, int createdBy, CancellationToken ct = default);
    Task<ServiceResult<ClassResponse>> UpdateAsync(int id, UpdateClassRequest request, CancellationToken ct = default);
    Task<ServiceResult<ClassResponse>> UpdateStatusAsync(int id, UpdateClassStatusRequest request, CancellationToken ct = default);

    // ── Enrollment ────────────────────────────────────────────────────────────
    Task<IEnumerable<ClassStudentResponse>> GetStudentsAsync(int classId, CancellationToken ct = default);
    Task<ServiceResult<IEnumerable<ClassStudentResponse>>> EnrollStudentsAsync(int classId, EnrollStudentsRequest request, CancellationToken ct = default);
    Task<ServiceResult<object?>> UnenrollStudentAsync(int classId, int studentId, CancellationToken ct = default);
}
