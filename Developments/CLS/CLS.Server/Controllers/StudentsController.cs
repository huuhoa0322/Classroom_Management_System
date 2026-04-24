using CLS.BLL.Common;
using CLS.BLL.DTOs.Payments;
using CLS.BLL.DTOs.Students;
using CLS.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CLS.Server.Controllers;

/// <summary>
/// API quản lý hồ sơ học sinh.
/// CLS-001: Onboard New Student Profiles
/// CLS-002: Update Student Lifecycles
/// </summary>
[ApiController]
[Route("api/v1/students")]
[Authorize(Roles = "Admin")]
[ProducesResponseType(typeof(ApiResponse<object>), 401)]   // Chưa đăng nhập
[ProducesResponseType(typeof(ApiResponse<object>), 403)]   // Không đủ quyền (không phải Admin)
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly IPaymentService _paymentService;

    public StudentsController(IStudentService studentService, IPaymentService paymentService)
    {
        _studentService = studentService;
        _paymentService = paymentService;
    }

    // ── GET /api/v1/students ──────────────────────────────────────────────────
    /// <summary>Lấy danh sách học sinh có phân trang và lọc theo trạng thái.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StudentResponse>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = AppConstants.Pagination.DefaultPage,
        [FromQuery] int pageSize = AppConstants.Pagination.DefaultPageSize,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var result = await _studentService.GetAllAsync(page, pageSize, status, ct);
        return Ok(ApiResponse<PagedResult<StudentResponse>>.Success(result, "Lấy danh sách học sinh thành công."));
    }

    // ── GET /api/v1/students/{id} ─────────────────────────────────────────────
    /// <summary>Lấy chi tiết một học sinh kèm thông tin phụ huynh.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var result = await _studentService.GetByIdAsync(id, ct);
        return this.ToOkResponse(result, "Lấy thông tin học sinh thành công.");
    }

    // ── POST /api/v1/students ─────────────────────────────────────────────────
    /// <summary>Tạo mới hồ sơ học sinh và tự động upsert phụ huynh (CLS-001).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request, CancellationToken ct = default)
    {
        var result = await _studentService.CreateAsync(request, ct);
        return this.ToCreatedAtActionResponse(
            result,
            nameof(GetById),
            student => new { id = student.Id },
            "Tạo hồ sơ học sinh thành công.");
    }

    // ── PUT /api/v1/students/{id} ─────────────────────────────────────────────
    /// <summary>Cập nhật thông tin cá nhân học sinh (CLS-002).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]   // Validation error
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentRequest request, CancellationToken ct = default)
    {
        var result = await _studentService.UpdateAsync(id, request, ct);
        return this.ToOkResponse(result, "Cập nhật học sinh thành công.");
    }

    // ── PATCH /api/v1/students/{id}/status ───────────────────────────────────
    /// <summary>Thay đổi trạng thái vòng đời học sinh: active ↔ inactive (CLS-002 AC1).</summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]   // Invalid status value
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> UpdateStatus(
        int id, [FromBody] UpdateStudentStatusRequest request, CancellationToken ct = default)
    {
        var result = await _studentService.UpdateStatusAsync(id, request, ct);
        return this.ToOkResponse(result, $"Cập nhật trạng thái học sinh thành '{request.Status}' thành công.");
    }

    // ── GET /api/v1/students/{id}/packages ───────────────────────────────────
    /// <summary>Lấy danh sách gói học của học sinh (financial dashboard — CLS-003).</summary>
    [HttpGet("{id:int}/packages")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StudentPackageResponse>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetStudentPackages(int id, CancellationToken ct = default)
    {
        var result = await _paymentService.GetStudentPackagesAsync(id, ct);
        return Ok(ApiResponse<IEnumerable<StudentPackageResponse>>.Success(result,
            "Lấy danh sách gói học của học sinh thành công."));
    }
}
