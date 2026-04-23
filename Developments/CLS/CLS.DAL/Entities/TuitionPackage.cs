namespace CLS.DAL.Entities;

/// <summary>
/// Danh mục gói học thuật (Tuition Package Catalog).
/// Pre-defined bởi Admin, dùng để gán khi tạo StudentPackage.
/// Bảng: packages
/// </summary>
public class TuitionPackage : BaseEntity
{
    /// <summary>Tên gói (e.g. "Gói 30 buổi").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Tổng số buổi học trong gói.</summary>
    public int TotalSessions { get; set; }

    /// <summary>Thời hạn sử dụng gói (ngày).</summary>
    public int DurationDays { get; set; }

    /// <summary>Giá gói (VNĐ).</summary>
    public decimal Price { get; set; }

    /// <summary>Trạng thái gói: active / inactive. Gói inactive không hiển thị khi chọn.</summary>
    public string Status { get; set; } = "active";

    // ── Navigation Properties ────────────────────────────────────────────
    /// <summary>Danh sách StudentPackage đã được gán gói này.</summary>
    public ICollection<StudentPackage> StudentPackages { get; set; } = new List<StudentPackage>();
}
