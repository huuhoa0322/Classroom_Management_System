using AutoMapper;
using CLS.BLL.Common;
using CLS.BLL.DTOs.StudentPackages;
using CLS.BLL.Interfaces;
using CLS.DAL.Entities;
using CLS.DAL.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CLS.BLL.Services;

/// <summary>Service xử lý nghiệp vụ quản lý gói học phí.</summary>
public class PackageService : IPackageService
{
    private readonly ITuitionPackageRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<PackageService> _logger;
    private readonly IValidator<CreatePackageRequest> _createValidator;
    private readonly IValidator<UpdatePackageRequest> _updateValidator;

    public PackageService(
        ITuitionPackageRepository repo, IMapper mapper, ILogger<PackageService> logger,
        IValidator<CreatePackageRequest> createValidator, IValidator<UpdatePackageRequest> updateValidator)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>Lấy danh sách gói phân trang.</summary>
    public async Task<PagedResult<PackageResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await _repo.GetPagedAsync(page, pageSize, ct);
        return PagedResult<PackageResponse>.Create(_mapper.Map<List<PackageResponse>>(items), total, page, pageSize);
    }

    /// <summary>Lấy chi tiết gói theo ID.</summary>
    public async Task<ServiceResult<PackageResponse>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return ServiceResult<PackageResponse>.Fail($"Gói #{id} không tồn tại.", 404);
        return ServiceResult<PackageResponse>.Success(_mapper.Map<PackageResponse>(entity));
    }

    /// <summary>Tạo gói học mới — kiểm tra tên trùng.</summary>
    public async Task<ServiceResult<PackageResponse>> CreateAsync(CreatePackageRequest request, CancellationToken ct = default)
    {
        var v = await _createValidator.ValidateAsync(request, ct);
        if (!v.IsValid) return ServiceResult<PackageResponse>.Validation(v.Errors);

        if (await _repo.ExistsByNameAsync(request.Name, null, ct))
            return ServiceResult<PackageResponse>.Fail($"Gói '{request.Name}' đã tồn tại.", 409);

        var entity = new TuitionPackage
        {
            Name = request.Name.Trim(),
            TotalSessions = request.TotalSessions,
            DurationDays = request.DurationDays,
            Price = request.Price,
            Status = AppConstants.TuitionPackageStatus.Active,
        };

        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);
        _logger.LogInformation("Created Package {PackageId}: {Name}", entity.Id, entity.Name);
        return ServiceResult<PackageResponse>.Success(_mapper.Map<PackageResponse>(entity));
    }

    /// <summary>Cập nhật thông tin gói — kiểm tra tên trùng (loại trừ chính nó).</summary>
    public async Task<ServiceResult<PackageResponse>> UpdateAsync(int id, UpdatePackageRequest request, CancellationToken ct = default)
    {
        var v = await _updateValidator.ValidateAsync(request, ct);
        if (!v.IsValid) return ServiceResult<PackageResponse>.Validation(v.Errors);

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return ServiceResult<PackageResponse>.Fail($"Gói #{id} không tồn tại.", 404);

        if (await _repo.ExistsByNameAsync(request.Name, id, ct))
            return ServiceResult<PackageResponse>.Fail($"Gói '{request.Name}' đã tồn tại.", 409);

        entity.Name = request.Name.Trim();
        entity.TotalSessions = request.TotalSessions;
        entity.DurationDays = request.DurationDays;
        entity.Price = request.Price;
        _repo.Update(entity);
        await _repo.SaveChangesAsync(ct);

        _logger.LogInformation("Updated Package {PackageId}: {Name}", id, entity.Name);
        return ServiceResult<PackageResponse>.Success(_mapper.Map<PackageResponse>(entity));
    }

    /// <summary>Đổi trạng thái gói: active ↔ inactive.</summary>
    public async Task<ServiceResult<PackageResponse>> UpdateStatusAsync(int id, UpdatePackageStatusRequest request, CancellationToken ct = default)
    {
        var valid = new[] { AppConstants.TuitionPackageStatus.Active, AppConstants.TuitionPackageStatus.Inactive };
        if (!valid.Contains(request.Status))
            return ServiceResult<PackageResponse>.Fail($"Trạng thái '{request.Status}' không hợp lệ.", 400);

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return ServiceResult<PackageResponse>.Fail($"Gói #{id} không tồn tại.", 404);

        entity.Status = request.Status;
        _repo.Update(entity);
        await _repo.SaveChangesAsync(ct);

        _logger.LogInformation("Updated Package {PackageId} status to {Status}", id, request.Status);
        return ServiceResult<PackageResponse>.Success(_mapper.Map<PackageResponse>(entity));
    }
}
