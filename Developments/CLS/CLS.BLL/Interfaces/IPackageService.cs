using CLS.BLL.Common;
using CLS.BLL.DTOs.StudentPackages;

namespace CLS.BLL.Interfaces;

/// <summary>Service interface cho quản lý gói học phí.</summary>
public interface IPackageService
{
    /// <summary>Lấy danh sách gói phân trang.</summary>
    Task<PagedResult<PackageResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>Lấy chi tiết gói theo ID.</summary>
    Task<ServiceResult<PackageResponse>> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Tạo gói mới.</summary>
    Task<ServiceResult<PackageResponse>> CreateAsync(CreatePackageRequest request, CancellationToken ct = default);

    /// <summary>Cập nhật thông tin gói.</summary>
    Task<ServiceResult<PackageResponse>> UpdateAsync(int id, UpdatePackageRequest request, CancellationToken ct = default);

    /// <summary>Đổi trạng thái gói.</summary>
    Task<ServiceResult<PackageResponse>> UpdateStatusAsync(int id, UpdatePackageStatusRequest request, CancellationToken ct = default);
}
