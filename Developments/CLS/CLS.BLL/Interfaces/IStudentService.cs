using CLS.BLL.Common;
using CLS.BLL.DTOs.Students;

namespace CLS.BLL.Interfaces;

public interface IStudentService
{
    Task<PagedResult<StudentResponse>> GetAllAsync(int page, int pageSize, string? status, CancellationToken ct = default);
    Task<StudentResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<StudentResponse> CreateAsync(CreateStudentRequest request, CancellationToken ct = default);
    Task<StudentResponse> UpdateAsync(int id, UpdateStudentRequest request, CancellationToken ct = default);
    Task<StudentResponse> UpdateStatusAsync(int id, UpdateStudentStatusRequest request, CancellationToken ct = default);
}
