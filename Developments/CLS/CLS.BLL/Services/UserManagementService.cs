using AutoMapper;
using CLS.BLL.Common;
using CLS.BLL.DTOs.Users;
using CLS.BLL.Interfaces;
using CLS.DAL.Entities;
using CLS.DAL.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CLS.BLL.Services;

/// <summary>Service xử lý nghiệp vụ quản lý tài khoản (Teacher).</summary>
public class UserManagementService : IUserManagementService
{
    private readonly IUserRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<UserManagementService> _logger;
    private readonly IValidator<CreateUserRequest> _createValidator;
    private readonly IValidator<UpdateUserRequest> _updateValidator;

    public UserManagementService(
        IUserRepository repo, IMapper mapper, ILogger<UserManagementService> logger,
        IValidator<CreateUserRequest> createValidator, IValidator<UpdateUserRequest> updateValidator)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>Lấy danh sách tài khoản phân trang.</summary>
    public async Task<PagedResult<UserResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await _repo.GetPagedAsync(page, pageSize, ct);
        return PagedResult<UserResponse>.Create(_mapper.Map<List<UserResponse>>(items), total, page, pageSize);
    }

    /// <summary>Lấy chi tiết tài khoản theo ID.</summary>
    public async Task<ServiceResult<UserResponse>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return ServiceResult<UserResponse>.Fail($"Tài khoản #{id} không tồn tại.", 404);
        return ServiceResult<UserResponse>.Success(_mapper.Map<UserResponse>(entity));
    }

    /// <summary>Tạo tài khoản Teacher — Role luôn là Teacher.</summary>
    public async Task<ServiceResult<UserResponse>> CreateTeacherAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var v = await _createValidator.ValidateAsync(request, ct);
        if (!v.IsValid) return ServiceResult<UserResponse>.Validation(v.Errors);

        if (await _repo.ExistsByEmailAsync(request.Email, null, ct))
            return ServiceResult<UserResponse>.Fail($"Email '{request.Email}' đã được sử dụng.", 409);

        var entity = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Phone = request.Phone?.Trim(),
            Role = AppConstants.AppRoles.Teacher,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Status = AppConstants.UserAccountStatus.Active,
        };

        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        _logger.LogInformation("Created Teacher account {UserId}: {Email}", entity.Id, entity.Email);
        return ServiceResult<UserResponse>.Success(_mapper.Map<UserResponse>(entity));
    }

    /// <summary>Cập nhật thông tin tài khoản — kiểm tra email trùng.</summary>
    public async Task<ServiceResult<UserResponse>> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var v = await _updateValidator.ValidateAsync(request, ct);
        if (!v.IsValid) return ServiceResult<UserResponse>.Validation(v.Errors);

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return ServiceResult<UserResponse>.Fail($"Tài khoản #{id} không tồn tại.", 404);

        if (await _repo.ExistsByEmailAsync(request.Email, id, ct))
            return ServiceResult<UserResponse>.Fail($"Email '{request.Email}' đã được sử dụng.", 409);

        entity.FullName = request.FullName.Trim();
        entity.Email = request.Email.Trim().ToLowerInvariant();
        entity.Phone = request.Phone?.Trim();
        _repo.Update(entity);
        await _repo.SaveChangesAsync(ct);

        _logger.LogInformation("Updated User {UserId}: {Email}", id, entity.Email);
        return ServiceResult<UserResponse>.Success(_mapper.Map<UserResponse>(entity));
    }

    /// <summary>Đổi trạng thái tài khoản: active ↔ inactive.</summary>
    public async Task<ServiceResult<UserResponse>> UpdateStatusAsync(int id, UpdateUserStatusRequest request, CancellationToken ct = default)
    {
        var valid = new[] { AppConstants.UserAccountStatus.Active, AppConstants.UserAccountStatus.Inactive };
        if (!valid.Contains(request.Status))
            return ServiceResult<UserResponse>.Fail($"Trạng thái '{request.Status}' không hợp lệ.", 400);

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return ServiceResult<UserResponse>.Fail($"Tài khoản #{id} không tồn tại.", 404);

        entity.Status = request.Status;
        _repo.Update(entity);
        await _repo.SaveChangesAsync(ct);

        _logger.LogInformation("Updated User {UserId} status to {Status}", id, request.Status);
        return ServiceResult<UserResponse>.Success(_mapper.Map<UserResponse>(entity));
    }

    /// <summary>Reset password ngẫu nhiên — trả về mật khẩu mới cho Admin gửi cho Teacher.</summary>
    public async Task<ServiceResult<ResetPasswordResponse>> ResetPasswordAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return ServiceResult<ResetPasswordResponse>.Fail($"Tài khoản #{id} không tồn tại.", 404);

        var newPassword = GenerateRandomPassword();
        entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _repo.Update(entity);
        await _repo.SaveChangesAsync(ct);

        _logger.LogInformation("Reset password for User {UserId}", id);
        return ServiceResult<ResetPasswordResponse>.Success(new ResetPasswordResponse { NewPassword = newPassword });
    }

    private static string GenerateRandomPassword(int length = 10)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789@#$";
        var random = new Random();
        return new string(Enumerable.Range(0, length).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
}
