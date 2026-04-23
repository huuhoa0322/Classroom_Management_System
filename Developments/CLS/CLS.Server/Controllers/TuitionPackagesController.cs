using CLS.BLL.Common;
using CLS.BLL.DTOs.Payments;
using CLS.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CLS.Server.Controllers;

/// <summary>
/// API danh mục gói học — catalog để Admin chọn khi tạo payment.
/// </summary>
[ApiController]
[Route("api/v1/tuition-packages")]
[Authorize(Roles = "Admin")]
[ProducesResponseType(typeof(ApiResponse<object>), 401)]
[ProducesResponseType(typeof(ApiResponse<object>), 403)]
public class TuitionPackagesController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public TuitionPackagesController(IPaymentService paymentService)
        => _paymentService = paymentService;

    // ── GET /api/v1/tuition-packages ──────────────────────────────────────────
    /// <summary>Lấy danh sách gói học đang active (cho dropdown chọn gói).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TuitionPackageDto>>), 200)]
    public async Task<IActionResult> GetAvailablePackages(CancellationToken ct = default)
    {
        var result = await _paymentService.GetAvailablePackagesAsync(ct);
        return Ok(ApiResponse<IEnumerable<TuitionPackageDto>>.Success(result,
            "Lấy danh sách gói học thành công."));
    }
}
