using AutoMapper;
using CLS.BLL.Common;
using CLS.BLL.DTOs.ActivityLogs;
using CLS.BLL.Interfaces;
using CLS.DAL.Entities;
using CLS.DAL.Repositories;
using Microsoft.Extensions.Logging;

namespace CLS.BLL.Services;

/// <summary>Service xử lý đọc + ghi Activity Logs.</summary>
public class ActivityLogService : IActivityLogService
{
    private readonly IActivityLogRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<ActivityLogService> _logger;

    public ActivityLogService(
        IActivityLogRepository repo,
        IMapper mapper,
        ILogger<ActivityLogService> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>Lấy danh sách log phân trang với bộ lọc.</summary>
    public async Task<PagedResult<ActivityLogResponse>> GetAllAsync(
        int page, int pageSize,
        int? userId = null,
        string? actionType = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken ct = default)
    {
        (page, pageSize) = AppConstants.Pagination.Clamp(page, pageSize);
        var (items, total) = await _repo.GetPagedAsync(page, pageSize, userId, actionType, from, to, ct);
        return PagedResult<ActivityLogResponse>.Create(
            _mapper.Map<List<ActivityLogResponse>>(items), total, page, pageSize);
    }

    /// <summary>Ghi một log entry mới (append-only).</summary>
    public async Task LogAsync(int userId, string actionType, string? description = null, CancellationToken ct = default)
    {
        var log = new ActivityLog
        {
            UserId = userId,
            ActionType = actionType,
            Description = description
        };

        await _repo.AddAsync(log, ct);
        await _repo.SaveChangesAsync(ct);

        _logger.LogInformation("ActivityLog: User {UserId} performed {ActionType}", userId, actionType);
    }
}
