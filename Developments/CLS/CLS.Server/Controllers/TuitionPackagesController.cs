using CLS.BLL.Common;
using CLS.BLL.DTOs.StudentPackages;
using CLS.BLL.DTOs.Payments;
using CLS.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CLS.Server.Controllers;

/// <summary>
/// API quản lý gói học — CRUD + dropdown catalog.
/// </summary>
[ApiController]
[Route("api/v1/tuition-packages")]
[Authorize(Roles = "Admin")]
public class TuitionPackagesController : ControllerBase
{
    private readonly IPackageService _packageService;
    private readonly IPaymentService _paymentService;
    private readonly IActivityLogService _activityLogService;

    public TuitionPackagesController(IPackageService packageService, IPaymentService paymentService, IActivityLogService activityLogService)
    {
        _packageService = packageService;
        _paymentService = paymentService;
        _activityLogService = activityLogService;
    }

    /// <summary>Lấy danh sách gói (phân trang).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PackageResponse>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = AppConstants.Pagination.DefaultPage,
        [FromQuery] int pageSize = AppConstants.Pagination.DefaultPageSize,
        CancellationToken ct = default)
    {
        var result = await _packageService.GetAllAsync(page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<PackageResponse>>.Success(result, "Lấy danh sách gói thành công."));
    }

    /// <summary>Lấy danh sách gói active (dropdown).</summary>
    [HttpGet("dropdown")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TuitionPackageDto>>), 200)]
    public async Task<IActionResult> GetDropdown(CancellationToken ct = default)
    {
        var result = await _paymentService.GetAvailablePackagesAsync(ct);
        return Ok(ApiResponse<IEnumerable<TuitionPackageDto>>.Success(result, "Lấy danh sách gói thành công."));
    }

    /// <summary>Tạo gói mới.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PackageResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> Create([FromBody] CreatePackageRequest request, CancellationToken ct = default)
    {
        var result = await _packageService.CreateAsync(request, ct);
        if (result.IsSuccess)
            this.LogActivity(_activityLogService, AppConstants.ActionTypes.Create, $"Tạo gói học: {request.Name}");
        return this.ToCreatedAtActionResponse(result, nameof(GetAll), _ => new { }, "Tạo gói thành công.");
    }

    /// <summary>Cập nhật gói.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PackageResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePackageRequest request, CancellationToken ct = default)
    {
        var result = await _packageService.UpdateAsync(id, request, ct);
        if (result.IsSuccess)
            this.LogActivity(_activityLogService, AppConstants.ActionTypes.Update, $"Cập nhật gói #{id}: {request.Name}");
        return this.ToOkResponse(result, "Cập nhật gói thành công.");
    }

    /// <summary>Đổi trạng thái gói: active ↔ inactive.</summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<PackageResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdatePackageStatusRequest request, CancellationToken ct = default)
    {
        var result = await _packageService.UpdateStatusAsync(id, request, ct);
        if (result.IsSuccess)
            this.LogActivity(_activityLogService, AppConstants.ActionTypes.StatusChange, $"Đổi trạng thái gói #{id} → {request.Status}");
        return this.ToOkResponse(result, $"Cập nhật trạng thái gói thành '{request.Status}' thành công.");
    }
}
