using CLS.BLL.Common;
using CLS.BLL.DTOs.Users;

namespace CLS.BLL.Interfaces;

/// <summary>Service interface cho quản lý tài khoản (Teacher).</summary>
public interface IUserManagementService
{
    /// <summary>Lấy danh sách tài khoản phân trang.</summary>
    Task<PagedResult<UserResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>Lấy chi tiết tài khoản theo ID.</summary>
    Task<ServiceResult<UserResponse>> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Tạo tài khoản Teacher mới.</summary>
    Task<ServiceResult<UserResponse>> CreateTeacherAsync(CreateUserRequest request, CancellationToken ct = default);

    /// <summary>Cập nhật thông tin tài khoản.</summary>
    Task<ServiceResult<UserResponse>> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct = default);

    /// <summary>Đổi trạng thái tài khoản.</summary>
    Task<ServiceResult<UserResponse>> UpdateStatusAsync(int id, UpdateUserStatusRequest request, CancellationToken ct = default);

    /// <summary>Đặt lại mật khẩu ngẫu nhiên.</summary>
    Task<ServiceResult<ResetPasswordResponse>> ResetPasswordAsync(int id, CancellationToken ct = default);
}
