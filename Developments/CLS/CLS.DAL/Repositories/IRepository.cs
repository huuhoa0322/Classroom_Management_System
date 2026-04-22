using CLS.DAL.Entities;

namespace CLS.DAL.Repositories;

/// <summary>
/// Generic base repository interface — CRUD cơ bản cho mọi Entity.
/// Các repository cụ thể extend interface này để thêm query đặc thù.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Delete(T entity);
}
