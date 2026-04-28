using CLS.DAL.Entities;

namespace CLS.DAL.Repositories;

public interface IClassRepository : IRepository<Class>
{
    /// <summary>Lấy tất cả lớp active (cho dropdown).</summary>
    Task<List<Class>> GetAllActiveAsync(CancellationToken ct = default);

    /// <summary>Lấy danh sách lớp phân trang, include ClassStudents + Sessions để đếm.</summary>
    Task<(List<Class> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>Lấy chi tiết lớp kèm ClassStudents.Student và Sessions — có tracking (cho Update/Enroll).</summary>
    Task<Class?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);

    /// <summary>Lấy chi tiết lớp kèm ClassStudents.Student và Sessions — AsNoTracking (read-only).</summary>
    Task<Class?> GetByIdWithDetailsReadOnlyAsync(int id, CancellationToken ct = default);

    /// <summary>Kiểm tra tên lớp đã tồn tại (exclude id khi update).</summary>
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken ct = default);
}

