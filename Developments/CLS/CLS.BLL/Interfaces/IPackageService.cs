using CLS.BLL.Common;
using CLS.BLL.DTOs.Packages;

namespace CLS.BLL.Interfaces;

public interface IPackageService
{
    Task<PagedResult<PackageResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<ServiceResult<PackageResponse>> CreateAsync(CreatePackageRequest request, CancellationToken ct = default);
    Task<ServiceResult<PackageResponse>> UpdateAsync(int id, UpdatePackageRequest request, CancellationToken ct = default);
    Task<ServiceResult<PackageResponse>> UpdateStatusAsync(int id, UpdatePackageStatusRequest request, CancellationToken ct = default);
}
