using CLS.BLL.Common;
using CLS.BLL.DTOs.Rooms;

namespace CLS.BLL.Interfaces;

/// <summary>Service interface cho quản lý phòng học.</summary>
public interface IRoomService
{
    /// <summary>Lấy danh sách phòng phân trang.</summary>
    Task<PagedResult<RoomResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>Lấy chi tiết phòng theo ID.</summary>
    Task<ServiceResult<RoomResponse>> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Tạo phòng mới.</summary>
    Task<ServiceResult<RoomResponse>> CreateAsync(CreateRoomRequest request, CancellationToken ct = default);

    /// <summary>Cập nhật thông tin phòng.</summary>
    Task<ServiceResult<RoomResponse>> UpdateAsync(int id, UpdateRoomRequest request, CancellationToken ct = default);

    /// <summary>Đổi trạng thái phòng.</summary>
    Task<ServiceResult<RoomResponse>> UpdateStatusAsync(int id, UpdateRoomStatusRequest request, CancellationToken ct = default);
}
