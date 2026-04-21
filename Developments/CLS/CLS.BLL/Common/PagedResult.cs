namespace CLS.BLL.Common;

/// <summary>
/// Pagination wrapper cho tất cả list endpoints của CLS.
/// Được dùng làm Data của ApiResponse khi trả về danh sách có phân trang.
///
/// JSON format (L2 mục 3.2 — collection resource):
/// {
///   "items": [...],
///   "totalCount": 87,
///   "page": 1,
///   "pageSize": 20,
///   "totalPages": 5
/// }
///
/// Ví dụ dùng trong Controller:
///   var paged = PagedResult&lt;StudentDto&gt;.Create(items, totalCount, page, pageSize);
///   return Ok(ApiResponse&lt;PagedResult&lt;StudentDto&gt;&gt;.Success(paged, "Students retrieved successfully"));
/// </summary>
public class PagedResult<T>
{
    /// <summary>Danh sách items của trang hiện tại.</summary>
    public List<T> Items { get; init; } = [];

    /// <summary>Tổng số records trong toàn bộ dataset (không phải chỉ trang hiện tại).</summary>
    public int TotalCount { get; init; }

    /// <summary>Trang hiện tại — 1-based (trang đầu tiên = 1).</summary>
    public int Page { get; init; }

    /// <summary>Số items mỗi trang. Default theo API rules: 20, max: 100.</summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Tổng số trang — tính tự động từ TotalCount và PageSize.
    /// Ví dụ: 87 items, pageSize 20 → 5 trang.
    /// </summary>
    public int TotalPages { get; init; }

    // ── Constructor (private — dùng static factory method) ───────────────────
    private PagedResult() { }

    // ── Static Factory Method ─────────────────────────────────────────────────

    /// <summary>
    /// Tạo PagedResult từ danh sách items và metadata phân trang.
    /// TotalPages được tính tự động.
    /// </summary>
    /// <param name="items">Danh sách items của trang hiện tại (đã được slice từ DB).</param>
    /// <param name="totalCount">Tổng số records trong toàn bộ dataset.</param>
    /// <param name="page">Trang hiện tại (1-based).</param>
    /// <param name="pageSize">Số items mỗi trang.</param>
    public static PagedResult<T> Create(
        IEnumerable<T> items,
        int totalCount,
        int page,
        int pageSize)
    {
        var totalPages = pageSize > 0
            ? (int)Math.Ceiling(totalCount / (double)pageSize)
            : 0;

        return new PagedResult<T>
        {
            Items      = items.ToList(),
            TotalCount = totalCount,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = totalPages
        };
    }
}
