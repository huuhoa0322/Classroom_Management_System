using CLS.DAL.Entities;

namespace CLS.DAL.Repositories;

public interface IRoomRepository : IRepository<Room>
{
    /// <summary>Lấy tất cả phòng active (cho dropdown).</summary>
    Task<List<Room>> GetAllActiveAsync(CancellationToken ct = default);
}
