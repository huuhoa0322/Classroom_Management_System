using AutoMapper;
using CLS.BLL.Common;
using CLS.BLL.DTOs.RenewalAlerts;
using CLS.BLL.Interfaces;
using CLS.DAL.Entities;
using CLS.DAL.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CLS.BLL.Services;

public class RenewalAlertService : IRenewalAlertService
{
    private readonly IRenewalAlertRepository _alertRepo;
    private readonly IStudentPackageRepository _spRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<RenewalAlertService> _logger;
    private readonly IValidator<UpdateAlertStatusRequest> _statusValidator;

    public RenewalAlertService(
        IRenewalAlertRepository alertRepo,
        IStudentPackageRepository spRepo,
        IMapper mapper,
        ILogger<RenewalAlertService> logger,
        IValidator<UpdateAlertStatusRequest> statusValidator)
    {
        _alertRepo = alertRepo;
        _spRepo = spRepo;
        _mapper = mapper;
        _logger = logger;
        _statusValidator = statusValidator;
    }

    // ── UC-06: Get Alerts (Paginated, Sortable, Filterable) ──────────────────
    public async Task<PagedResult<RenewalAlertResponse>> GetAlertsAsync(
        int page, int pageSize,
        string? status, string? sortBy, string? sortDir,
        CancellationToken ct = default)
    {
        page = Math.Max(page, AppConstants.Pagination.DefaultPage);
        pageSize = Math.Clamp(pageSize, 1, AppConstants.Pagination.MaxPageSize);

        var alerts = await _alertRepo.GetAlertsPagedAsync(page, pageSize, status, sortBy, sortDir, ct);
        var totalCount = await _alertRepo.CountAsync(status, ct);

        var dtos = _mapper.Map<List<RenewalAlertResponse>>(alerts);

        _logger.LogInformation(
            "Retrieved {Count} renewal alerts (page {Page}, status={Status})",
            dtos.Count, page, status ?? "all");

        return PagedResult<RenewalAlertResponse>.Create(dtos, totalCount, page, pageSize);
    }

    // ── UC-06: Toggle Alert Status ───────────────────────────────────────────
    public async Task<ServiceResult<RenewalAlertResponse>> UpdateAlertStatusAsync(
        int alertId, UpdateAlertStatusRequest request,
        CancellationToken ct = default)
    {
        // Validate
        var validation = await _statusValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return ServiceResult<RenewalAlertResponse>.Validation(validation.Errors);

        // Find
        var alert = await _alertRepo.GetByIdAsync(alertId, ct);
        if (alert is null)
            return ServiceResult<RenewalAlertResponse>.Fail(
                $"Không tìm thấy cảnh báo với ID {alertId}.", 404);

        // Update status
        alert.Status = request.Status;
        alert.ConsultedAt = request.Status == AppConstants.AlertNotificationStatus.Consulted
            ? DateTime.UtcNow
            : null;

        _alertRepo.Update(alert);
        await _alertRepo.SaveChangesAsync(ct);

        var dto = _mapper.Map<RenewalAlertResponse>(alert);

        _logger.LogInformation(
            "Alert {AlertId} status updated to {Status} for StudentPackage {SpId}",
            alertId, request.Status, alert.StudentPackageId);

        return ServiceResult<RenewalAlertResponse>.Success(dto);
    }

    // ── UC-10: Scan & Create Renewal Alerts ──────────────────────────────────
    public async Task<int> ScanAndCreateAlertsAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting depletion scan for near-expiry student packages...");

        // Query 1: Lấy tất cả active packages kèm Student → Parent (single query)
        var activePackages = await _spRepo.GetActiveWithDetailsAsync(ct);

        // Query 2: Lấy tất cả package IDs đã có alert BẤT KỲ status (single query — tránh duplicate)
        var existingAlertPackageIds = await _alertRepo.GetExistingAlertPackageIdsAsync(ct);

        var newAlerts = new List<AlertNotification>();

        foreach (var sp in activePackages)
        {
            var remainingDays = (sp.EndDate.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).Days;
            var isLowSessions = sp.RemainingSessions <= AppConstants.DepletionThresholds.MinSessions;
            var isNearExpiry = remainingDays <= AppConstants.DepletionThresholds.MinDays;

            if (!isLowSessions && !isNearExpiry) continue;

            // O(1) lookup thay vì DB call per-loop
            if (existingAlertPackageIds.Contains(sp.Id)) continue;

            // Student → Parent đã được eager-load ở Query 1
            if (sp.Student?.Parent is null) continue;

            var reason = isLowSessions
                ? $"Gói học còn {sp.RemainingSessions} buổi (ngưỡng: ≤{AppConstants.DepletionThresholds.MinSessions})"
                : $"Gói học còn {remainingDays} ngày trước khi hết hạn (ngưỡng: ≤{AppConstants.DepletionThresholds.MinDays} ngày)";

            newAlerts.Add(new AlertNotification
            {
                StudentPackageId = sp.Id,
                TargetEmail = sp.Student.Parent.Email,
                Type = AppConstants.AlertNotificationType.RenewalAlert,
                Message = reason,
                Status = AppConstants.AlertNotificationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (newAlerts.Count > 0)
        {
            await _alertRepo.AddRangeAsync(newAlerts, ct);
            await _alertRepo.SaveChangesAsync(ct);
        }

        _logger.LogInformation(
            "Depletion scan completed. Scanned {Total} active packages, created {New} new alerts.",
            activePackages.Count, newAlerts.Count);

        return newAlerts.Count;
    }
}
