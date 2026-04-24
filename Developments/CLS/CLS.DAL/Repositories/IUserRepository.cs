using CLS.DAL.Entities;

namespace CLS.DAL.Repositories;

/// <summary>
/// Repository cho User — quản lý tài khoản nội bộ (Admin, Teacher).
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>Lấy danh sách Teacher active (role = 'Teacher').</summary>
    Task<List<User>> GetTeachersAsync(CancellationToken ct = default);
}
