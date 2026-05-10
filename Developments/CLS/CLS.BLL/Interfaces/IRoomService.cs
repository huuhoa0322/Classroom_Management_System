using CLS.BLL.Common;
using CLS.BLL.DTOs.Rooms;

namespace CLS.BLL.Interfaces;

public interface IRoomService
{
    Task<PagedResult<RoomResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<ServiceResult<RoomResponse>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<RoomResponse>> CreateAsync(CreateRoomRequest request, CancellationToken ct = default);
    Task<ServiceResult<RoomResponse>> UpdateAsync(int id, UpdateRoomRequest request, CancellationToken ct = default);
    Task<ServiceResult<RoomResponse>> UpdateStatusAsync(int id, UpdateRoomStatusRequest request, CancellationToken ct = default);
}
