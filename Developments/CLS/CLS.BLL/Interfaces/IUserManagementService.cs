using CLS.BLL.Common;
using CLS.BLL.DTOs.Users;

namespace CLS.BLL.Interfaces;

public interface IUserManagementService
{
    Task<PagedResult<UserResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<ServiceResult<UserResponse>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<UserResponse>> CreateTeacherAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<ServiceResult<UserResponse>> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct = default);
    Task<ServiceResult<UserResponse>> UpdateStatusAsync(int id, UpdateUserStatusRequest request, CancellationToken ct = default);
    Task<ServiceResult<ResetPasswordResponse>> ResetPasswordAsync(int id, CancellationToken ct = default);
}
