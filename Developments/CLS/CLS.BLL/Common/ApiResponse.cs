namespace CLS.BLL.Common;

/// <summary>
/// Generic response envelope cho toàn bộ API của CLS.
/// MỌI response (success và error) đều phải được wrap qua class này.
///
/// JSON format bắt buộc (L2 mục 3):
/// { "code": 200, "message": "...", "data": { ... } }
///
/// Lưu ý: camelCase serialization được cấu hình tại Program.cs
/// (JsonNamingPolicy.CamelCase) — không cần JsonPropertyName attribute.
/// </summary>
public class ApiResponse<T>
{
    /// <summary>HTTP status code (200, 201, 400, 401, 403, 404, 409, 422, 500).</summary>
    public int Code { get; init; }

    /// <summary>Human-readable result description.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Response payload. Null cho error hoặc no-content responses.</summary>
    public T? Data { get; init; }

    // ── Constructor (private — dùng static factory methods bên dưới) ──────────
    private ApiResponse() { }

    // ── Static Factory Methods ────────────────────────────────────────────────

    /// <summary>
    /// 200 OK — Dùng cho GET, PUT thành công.
    /// </summary>
    public static ApiResponse<T> Success(T data, string message = "Success")
        => new() { Code = 200, Message = message, Data = data };

    /// <summary>
    /// 201 Created — Dùng cho POST tạo resource mới thành công.
    /// </summary>
    public static ApiResponse<T> Created(T data, string message = "Created successfully")
        => new() { Code = 201, Message = message, Data = data };

    /// <summary>
    /// 200 OK, data = null — Dùng cho DELETE hoặc action không cần trả payload.
    /// </summary>
    public static ApiResponse<T> NoContent(string message = "Operation completed successfully")
        => new() { Code = 200, Message = message, Data = default };

    /// <summary>
    /// Error response — Dùng cho mọi loại lỗi. Code mặc định 400.
    /// Các error code thường dùng: 400 (Validation), 401 (Unauthorized),
    /// 403 (Forbidden), 404 (NotFound), 409 (Conflict), 422 (Business rule), 500 (Server).
    /// </summary>
    public static ApiResponse<T> Fail(string message, int code = 400, T? data = default)
        => new() { Code = code, Message = message, Data = data };
}

/// <summary>
/// Non-generic convenience overload — dùng cho error responses không cần type parameter.
///
/// Ví dụ trong Controller:
///   return NotFound(ApiResponse.Fail("Student not found", 404));
///   return StatusCode(500, ApiResponse.Fail("Unexpected error"));
/// </summary>
public static class ApiResponse
{
    public static ApiResponse<object?> Fail(string message, int code = 400, object? data = null)
        => ApiResponse<object?>.Fail(message, code, data);

    public static ApiResponse<object?> NoContent(string message = "Operation completed successfully")
        => ApiResponse<object?>.NoContent(message);
}
