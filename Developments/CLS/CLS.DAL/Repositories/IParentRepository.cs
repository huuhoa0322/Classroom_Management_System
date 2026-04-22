using CLS.DAL.Entities;

namespace CLS.DAL.Repositories;

public interface IParentRepository : IRepository<Parent>
{
    /// <summary>
    /// Tìm phụ huynh theo email (case-insensitive).
    /// Dùng để upsert khi tạo học sinh mới.
    /// </summary>
    Task<Parent?> GetByEmailAsync(string email, CancellationToken ct = default);
}
