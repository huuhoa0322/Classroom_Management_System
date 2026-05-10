using CLS.DAL.Entities;

namespace CLS.DAL.Repositories;

public interface IRoomRepository : IRepository<Room>
{
    /// <summary>Lấy tất cả phòng active (cho dropdown).</summary>
    Task<List<Room>> GetAllActiveAsync(CancellationToken ct = default);

    /// <summary>Lấy danh sách phòng phân trang.</summary>
    Task<(List<Room> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>Kiểm tra tên phòng đã tồn tại (exclude id khi update).</summary>
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken ct = default);
}
