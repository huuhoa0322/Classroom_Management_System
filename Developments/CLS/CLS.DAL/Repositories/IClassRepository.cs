using CLS.DAL.Entities;

namespace CLS.DAL.Repositories;

public interface IClassRepository : IRepository<Class>
{
    /// <summary>Lấy tất cả lớp active (cho dropdown).</summary>
    Task<List<Class>> GetAllActiveAsync(CancellationToken ct = default);
}
