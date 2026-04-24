using CLS.BLL.Common;
using CLS.BLL.DTOs.Payments;
using CLS.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CLS.Server.Controllers;

/// <summary>
/// API thanh toán offline — CLS-003: Record Offline Tuition Payments.
/// </summary>
[ApiController]
[Route("api/v1/payments")]
[Authorize(Roles = "Admin")]
[ProducesResponseType(typeof(ApiResponse<object>), 401)]
[ProducesResponseType(typeof(ApiResponse<object>), 403)]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
        => _paymentService = paymentService;

    // ── POST /api/v1/payments ─────────────────────────────────────────────────
    /// <summary>Ghi nhận thanh toán offline mới (status = pending, method = bank_transfer).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> RecordPayment(
        [FromBody] RecordPaymentRequest request, CancellationToken ct = default)
    {
        var adminUserId = GetCurrentUserId();
        var result = await _paymentService.RecordPaymentAsync(request, adminUserId, ct);
        return CreatedAtAction(nameof(RecordPayment), new { id = result.Id },
            ApiResponse<PaymentResponse>.Created(result, "Ghi nhận thanh toán thành công. Trạng thái: Chờ xác nhận."));
    }

    // ── PATCH /api/v1/payments/{id}/status ────────────────────────────────────
    /// <summary>Cập nhật trạng thái thanh toán (confirm / fail / refund).</summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> UpdatePaymentStatus(
        int id, [FromBody] UpdatePaymentStatusRequest request, CancellationToken ct = default)
    {
        var result = await _paymentService.UpdatePaymentStatusAsync(id, request, ct);
        return Ok(ApiResponse<PaymentResponse>.Success(result,
            $"Cập nhật trạng thái thanh toán thành '{request.Status}' thành công."));
    }

    // ── GET /api/v1/payments?studentId={id} ───────────────────────────────────
    /// <summary>Lấy lịch sử thanh toán của học sinh (phân trang).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentResponse>>), 200)]
    public async Task<IActionResult> GetPaymentsByStudent(
        [FromQuery] int studentId,
        [FromQuery] int page = AppConstants.Pagination.DefaultPage,
        [FromQuery] int pageSize = AppConstants.Pagination.DefaultPageSize,
        CancellationToken ct = default)
    {
        var result = await _paymentService.GetPaymentsByStudentAsync(studentId, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<PaymentResponse>>.Success(result,
            "Lấy lịch sử thanh toán thành công."));
    }

    // ── GET /api/v1/payments/all ──────────────────────────────────────────────
    /// <summary>Lấy toàn bộ lịch sử thanh toán (phân trang, không lọc student).</summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentResponse>>), 200)]
    public async Task<IActionResult> GetAllPayments(
        [FromQuery] int page = AppConstants.Pagination.DefaultPage,
        [FromQuery] int pageSize = AppConstants.Pagination.DefaultPageSize,
        CancellationToken ct = default)
    {
        var result = await _paymentService.GetAllPaymentsAsync(page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<PaymentResponse>>.Success(result,
            "Lấy toàn bộ lịch sử thanh toán thành công."));
    }

    // ── Helper: lấy userId từ JWT claims ──────────────────────────────────────
    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                    ?? User.FindFirst("sub");
        return claim is not null && int.TryParse(claim.Value, out var id) ? id : 0;
    }
}
