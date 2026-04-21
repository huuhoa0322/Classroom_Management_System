namespace CLS.DAL.Entities;

/// <summary>
/// Lớp cơ sở cho tất cả Entity trong hệ thống CLS.
/// Cung cấp: Primary Key (int Identity), Audit fields (CreatedAt, UpdatedAt),
/// và Soft Delete (IsDeleted, DeletedAt).
///
/// QUY TẮC: Tuyệt đối KHÔNG dùng Data Annotations trên class này hoặc các
/// subclass — toàn bộ cấu hình thông qua Fluent API trong IEntityTypeConfiguration.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Primary Key — auto-increment (PostgreSQL IDENTITY).</summary>
    public int Id { get; set; }

    /// <summary>
    /// Thời điểm tạo record (UTC). Set tự động trong AppDbContext.SaveChangesAsync.
    /// KHÔNG set thủ công trong Service/Controller.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời điểm cập nhật gần nhất (UTC). Set tự động trong AppDbContext.SaveChangesAsync.
    /// KHÔNG set thủ công trong Service/Controller.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Soft-delete flag. Khi true, record bị ẩn khỏi mọi query
    /// nhờ HasQueryFilter trong BaseEntityConfiguration.
    /// KHÔNG hard-delete record — chỉ set IsDeleted = true.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>Thời điểm soft-delete (UTC). Null nếu chưa bị xóa.</summary>
    public DateTime? DeletedAt { get; set; }
}
