using CLS.DAL.Entities;

namespace CLS.DAL.Repositories;

/// <summary>
/// Repository cho User — quản lý tài khoản nội bộ (Admin, Teacher).
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>Lấy danh sách Teacher active (role = 'Teacher').</summary>
    Task<List<User>> GetTeachersAsync(CancellationToken ct = default);

    /// <summary>Lấy danh sách user phân trang.</summary>
    Task<(List<User> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>Kiểm tra email đã tồn tại (exclude id khi update).</summary>
    Task<bool> ExistsByEmailAsync(string email, int? excludeId = null, CancellationToken ct = default);
}
