using FluentValidation.Results;

namespace CLS.BLL.Common;

/// <summary>
/// Result wrapper for expected application outcomes.
/// Use this for validation, not-found, and conflict paths so production
/// flows do not rely on exceptions for normal business feedback.
/// </summary>
public sealed class ServiceResult<T>
{
    private ServiceResult(
        bool isSuccess,
        T? value,
        string message,
        int statusCode,
        object? errorData)
    {
        IsSuccess = isSuccess;
        Value = value;
        Message = message;
        StatusCode = statusCode;
        ErrorData = errorData;
    }

    public bool IsSuccess { get; }
    public T? Value { get; }
    public string Message { get; }
    public int StatusCode { get; }
    public object? ErrorData { get; }

    public static ServiceResult<T> Success(T value)
        => new(true, value, string.Empty, 200, null);

    public static ServiceResult<T> Fail(string message, int statusCode, object? errorData = null)
        => new(false, default, message, statusCode, errorData);

    public static ServiceResult<T> Validation(IEnumerable<ValidationFailure> validationErrors)
    {
        var errors = validationErrors.ToList();
        var groupedErrors = errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(e => e.ErrorMessage).ToArray());

        var message = string.Join("; ", errors.Select(e => e.ErrorMessage));
        return Fail(message, 400, new { errors = groupedErrors });
    }
}
