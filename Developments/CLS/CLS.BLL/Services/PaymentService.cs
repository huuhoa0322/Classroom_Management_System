using AutoMapper;
using CLS.BLL.Common;
using CLS.BLL.Common.Exceptions;
using CLS.BLL.DTOs.Payments;
using CLS.BLL.Interfaces;
using CLS.DAL.Entities;
using CLS.DAL.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CLS.BLL.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository                    _paymentRepo;
    private readonly IStudentPackageRepository              _studentPackageRepo;
    private readonly ITuitionPackageRepository              _tuitionPackageRepo;
    private readonly IStudentRepository                    _studentRepo;
    private readonly IMapper                               _mapper;
    private readonly ILogger<PaymentService>                _logger;
    private readonly IValidator<RecordPaymentRequest>       _recordValidator;
    private readonly IValidator<UpdatePaymentStatusRequest> _updateStatusValidator;

    public PaymentService(
        IPaymentRepository paymentRepo,
        IStudentPackageRepository studentPackageRepo,
        ITuitionPackageRepository tuitionPackageRepo,
        IStudentRepository studentRepo,
        IMapper mapper,
        ILogger<PaymentService> logger,
        IValidator<RecordPaymentRequest> recordValidator,
        IValidator<UpdatePaymentStatusRequest> updateStatusValidator)
    {
        _paymentRepo            = paymentRepo;
        _studentPackageRepo     = studentPackageRepo;
        _tuitionPackageRepo     = tuitionPackageRepo;
        _studentRepo            = studentRepo;
        _mapper                 = mapper;
        _logger                 = logger;
        _recordValidator        = recordValidator;
        _updateStatusValidator  = updateStatusValidator;
    }

    // ── RECORD PAYMENT (CLS-003 AC1) ─────────────────────────────────────────
    public async Task<PaymentResponse> RecordPaymentAsync(
        RecordPaymentRequest request, int adminUserId, CancellationToken ct = default)
    {
        // BƯỚC 0: Validate
        var validation = await _recordValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new CLS.BLL.Common.Exceptions.ValidationException(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        // BƯỚC 1: Check Student tồn tại
        var student = await _studentRepo.GetByIdAsync(request.StudentId, ct)
            ?? throw new NotFoundException($"Học sinh #{request.StudentId} không tồn tại.");

        // BƯỚC 2: Check TuitionPackage tồn tại
        var package = await _tuitionPackageRepo.GetByIdAsync(request.TuitionPackageId, ct)
            ?? throw new NotFoundException($"Gói học #{request.TuitionPackageId} không tồn tại.");

        if (package.Status != "active")
            throw new CLS.BLL.Common.Exceptions.ValidationException(
                $"Gói học '{package.Name}' đã ngừng bán (inactive).");

        // BƯỚC 3: Tạo StudentPackage (pending_payment — chưa credit sessions)
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var studentPackage = new StudentPackage
        {
            StudentId         = request.StudentId,
            PackageId         = request.TuitionPackageId,
            StartDate         = startDate,
            EndDate           = startDate.AddDays(package.DurationDays),
            TotalSessions     = package.TotalSessions,
            RemainingSessions = 0, // Chưa credit — chờ confirm
            Status            = AppConstants.StudentPackageStatus.PendingPayment
        };
        await _studentPackageRepo.AddAsync(studentPackage, ct);

        // BƯỚC 4: Tạo Payment (pending)
        var payment = new Payment
        {
            StudentPackageId = 0, // sẽ được set sau SaveChanges qua navigation
            AdminId          = adminUserId,
            Amount           = request.AmountPaid,
            PaymentDate      = DateTime.UtcNow,
            PaymentMethod    = AppConstants.PaymentMethod.BankTransfer,
            Status           = AppConstants.PaymentStatus.Pending
        };
        // Liên kết qua navigation property (EF Core tự resolve FK)
        payment.StudentPackage = studentPackage;
        await _paymentRepo.AddAsync(payment, ct);

        // BƯỚC 5: Atomic save
        await _paymentRepo.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Recorded Payment {PaymentId} for Student {StudentId}, Package '{PackageName}' (pending)",
            payment.Id, request.StudentId, package.Name);

        // Reload với full details để map response
        var created = await _paymentRepo.GetByIdWithDetailsAsync(payment.Id, ct)
            ?? throw new InvalidOperationException($"Không thể reload Payment {payment.Id} sau khi tạo.");

        return _mapper.Map<PaymentResponse>(created);
    }

    // ── UPDATE PAYMENT STATUS ────────────────────────────────────────────────
    public async Task<PaymentResponse> UpdatePaymentStatusAsync(
        int paymentId, UpdatePaymentStatusRequest request, CancellationToken ct = default)
    {
        // Validate input trước khi query DB
        var validation = await _updateStatusValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new CLS.BLL.Common.Exceptions.ValidationException(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var payment = await _paymentRepo.GetByIdWithDetailsAsync(paymentId, ct)
            ?? throw new NotFoundException($"Thanh toán #{paymentId} không tồn tại.");

        var oldStatus = payment.Status;
        var newStatus = request.Status;

        // Validate transition
        ValidateStatusTransition(oldStatus, newStatus);

        var studentPackage = payment.StudentPackage;

        switch (newStatus)
        {
            case AppConstants.PaymentStatus.Confirmed:
                // Credit sessions vào StudentPackage
                studentPackage.RemainingSessions = studentPackage.TotalSessions;
                studentPackage.Status = AppConstants.StudentPackageStatus.Active;
                _logger.LogInformation(
                    "Payment {PaymentId} confirmed — credited {Sessions} sessions to StudentPackage {SpId}",
                    paymentId, studentPackage.TotalSessions, studentPackage.Id);
                break;

            case AppConstants.PaymentStatus.Failed:
            case AppConstants.PaymentStatus.Refunded:
                // Archive StudentPackage, reset sessions
                studentPackage.RemainingSessions = 0;
                studentPackage.Status = AppConstants.StudentPackageStatus.Archived;
                _logger.LogWarning(
                    "Payment {PaymentId} {NewStatus} — archived StudentPackage {SpId}",
                    paymentId, newStatus, studentPackage.Id);
                break;
        }

        payment.Status = newStatus;
        _paymentRepo.Update(payment);
        _studentPackageRepo.Update(studentPackage);
        await _paymentRepo.SaveChangesAsync(ct);

        _logger.LogInformation("Payment {PaymentId} status changed: {Old} → {New}",
            paymentId, oldStatus, newStatus);

        // Reload to get fresh data
        var updated = await _paymentRepo.GetByIdWithDetailsAsync(paymentId, ct);
        return _mapper.Map<PaymentResponse>(updated!);
    }

    // ── GET PAYMENTS BY STUDENT (phân trang) ─────────────────────────────────
    public async Task<PagedResult<PaymentResponse>> GetPaymentsByStudentAsync(
        int studentId, int page, int pageSize, CancellationToken ct = default)
    {
        var (payments, total) = await _paymentRepo.GetPagedByStudentIdAsync(studentId, page, pageSize, ct);
        var items = _mapper.Map<List<PaymentResponse>>(payments);
        return PagedResult<PaymentResponse>.Create(items, total, page, pageSize);
    }

    // ── GET ALL PAYMENTS (phân trang — global) ───────────────────────────────
    public async Task<PagedResult<PaymentResponse>> GetAllPaymentsAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var (payments, total) = await _paymentRepo.GetPagedAllAsync(page, pageSize, ct);
        var items = _mapper.Map<List<PaymentResponse>>(payments);
        return PagedResult<PaymentResponse>.Create(items, total, page, pageSize);
    }

    // ── GET STUDENT PACKAGES ─────────────────────────────────────────────────
    public async Task<IEnumerable<StudentPackageResponse>> GetStudentPackagesAsync(
        int studentId, CancellationToken ct = default)
    {
        var packages = await _studentPackageRepo.GetByStudentIdAsync(studentId, ct);
        return _mapper.Map<IEnumerable<StudentPackageResponse>>(packages);
    }

    // ── GET AVAILABLE PACKAGES (Catalog) ─────────────────────────────────────
    public async Task<IEnumerable<TuitionPackageDto>> GetAvailablePackagesAsync(
        CancellationToken ct = default)
    {
        var packages = await _tuitionPackageRepo.GetAllActiveAsync(ct);
        return _mapper.Map<IEnumerable<TuitionPackageDto>>(packages);
    }

    // ── HELPER: Validate payment status transitions ──────────────────────────
    private static void ValidateStatusTransition(string currentStatus, string newStatus)
    {
        var validTransitions = new Dictionary<string, string[]>
        {
            [AppConstants.PaymentStatus.Pending]   = [AppConstants.PaymentStatus.Confirmed, AppConstants.PaymentStatus.Failed],
            [AppConstants.PaymentStatus.Confirmed]  = [AppConstants.PaymentStatus.Refunded],
            [AppConstants.PaymentStatus.Failed]     = [],
            [AppConstants.PaymentStatus.Refunded]   = []
        };

        if (!validTransitions.TryGetValue(currentStatus, out var allowed) || !allowed.Contains(newStatus))
        {
            throw new ConflictException(
                $"Không thể chuyển trạng thái thanh toán từ '{currentStatus}' sang '{newStatus}'.");
        }
    }
}
