namespace CLS.BLL.Common.Exceptions;

/// <summary>
/// Abstract base class cho tất cả domain exceptions của CLS.
/// Middleware chỉ cần 1 catch block cho ClsException để xử lý
/// toàn bộ domain errors, tách biệt với unexpected server errors (500).
/// </summary>
public abstract class ClsException : Exception
{
    /// <summary>HTTP status code tương ứng với loại exception này.</summary>
    public int StatusCode { get; }

    protected ClsException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}
