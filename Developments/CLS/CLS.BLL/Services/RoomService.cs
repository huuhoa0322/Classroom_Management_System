using AutoMapper;
using CLS.BLL.Common;
using CLS.BLL.DTOs.Rooms;
using CLS.BLL.Interfaces;
using CLS.DAL.Entities;
using CLS.DAL.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CLS.BLL.Services;

/// <summary>Service xử lý nghiệp vụ quản lý phòng học.</summary>
public class RoomService : IRoomService
{
    private readonly IRoomRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<RoomService> _logger;
    private readonly IValidator<CreateRoomRequest> _createValidator;
    private readonly IValidator<UpdateRoomRequest> _updateValidator;

    public RoomService(
        IRoomRepository repo, IMapper mapper, ILogger<RoomService> logger,
        IValidator<CreateRoomRequest> createValidator, IValidator<UpdateRoomRequest> updateValidator)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>Lấy danh sách phòng phân trang.</summary>
    public async Task<PagedResult<RoomResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = AppConstants.Pagination.Clamp(page, pageSize);
        var (items, total) = await _repo.GetPagedAsync(page, pageSize, ct);
        return PagedResult<RoomResponse>.Create(_mapper.Map<List<RoomResponse>>(items), total, page, pageSize);
    }

    /// <summary>Lấy chi tiết phòng theo ID.</summary>
    public async Task<ServiceResult<RoomResponse>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return ServiceResult<RoomResponse>.Fail($"Phòng #{id} không tồn tại.", 404);
        return ServiceResult<RoomResponse>.Success(_mapper.Map<RoomResponse>(entity));
    }

    /// <summary>Tạo phòng mới — kiểm tra tên trùng.</summary>
    public async Task<ServiceResult<RoomResponse>> CreateAsync(CreateRoomRequest request, CancellationToken ct = default)
    {
        var v = await _createValidator.ValidateAsync(request, ct);
        if (!v.IsValid) return ServiceResult<RoomResponse>.Validation(v.Errors);

        if (await _repo.ExistsByNameAsync(request.Name, null, ct))
            return ServiceResult<RoomResponse>.Fail($"Phòng '{request.Name}' đã tồn tại.", 409);

        var entity = new Room { Name = request.Name.Trim(), Capacity = request.Capacity, Status = AppConstants.RoomStatus.Active };
        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        _logger.LogInformation("Created Room {RoomId}: {Name}", entity.Id, entity.Name);
        return ServiceResult<RoomResponse>.Success(_mapper.Map<RoomResponse>(entity));
    }

    /// <summary>Cập nhật thông tin phòng — kiểm tra tên trùng (loại trừ chính nó).</summary>
    public async Task<ServiceResult<RoomResponse>> UpdateAsync(int id, UpdateRoomRequest request, CancellationToken ct = default)
    {
        var v = await _updateValidator.ValidateAsync(request, ct);
        if (!v.IsValid) return ServiceResult<RoomResponse>.Validation(v.Errors);

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return ServiceResult<RoomResponse>.Fail($"Phòng #{id} không tồn tại.", 404);

        if (await _repo.ExistsByNameAsync(request.Name, id, ct))
            return ServiceResult<RoomResponse>.Fail($"Phòng '{request.Name}' đã tồn tại.", 409);

        entity.Name = request.Name.Trim();
        entity.Capacity = request.Capacity;
        _repo.Update(entity);
        await _repo.SaveChangesAsync(ct);

        _logger.LogInformation("Updated Room {RoomId}: {Name}", id, entity.Name);
        return ServiceResult<RoomResponse>.Success(_mapper.Map<RoomResponse>(entity));
    }

    /// <summary>Đổi trạng thái phòng: active ↔ inactive.</summary>
    public async Task<ServiceResult<RoomResponse>> UpdateStatusAsync(int id, UpdateRoomStatusRequest request, CancellationToken ct = default)
    {
        var valid = new[] { AppConstants.RoomStatus.Active, AppConstants.RoomStatus.Inactive };
        if (!valid.Contains(request.Status))
            return ServiceResult<RoomResponse>.Fail($"Trạng thái '{request.Status}' không hợp lệ.", 400);

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return ServiceResult<RoomResponse>.Fail($"Phòng #{id} không tồn tại.", 404);

        entity.Status = request.Status;
        _repo.Update(entity);
        await _repo.SaveChangesAsync(ct);

        _logger.LogInformation("Updated Room {RoomId} status to {Status}", id, request.Status);
        return ServiceResult<RoomResponse>.Success(_mapper.Map<RoomResponse>(entity));
    }
}
