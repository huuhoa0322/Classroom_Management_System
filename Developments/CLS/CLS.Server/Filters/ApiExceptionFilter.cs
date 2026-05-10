using System.Data.Common;
using CLS.BLL.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CLS.Server.Filters;

/// <summary>
/// Global MVC Action Filter — bọc toàn bộ controller action bằng try-catch THẬT SỰ.
///
/// ⚠️ Khác biệt quan trọng so với IExceptionFilter:
///   - IExceptionFilter.OnException() được MVC framework GỌI SAU KHI exception xảy ra
///     → VS debugger vẫn coi exception là "User-Unhandled" tại throw site
///   - IAsyncActionFilter.OnActionExecutionAsync() BỌC action bằng try-catch trực tiếp
///     → VS debugger coi exception là "handled" → KHÔNG break
///
/// Thứ tự ưu tiên:
///   1. ValidationException       → 400 + errors field
///   2. ClsException (domain)     → StatusCode từ exception (401, 404, 409, v.v.)
///   3. Transient DB Exception    → 503 (auto-detected NpgsqlException, SocketException, ...)
///   4. Exception (bất kỳ)        → 500 (không leak stack trace ra Production)
/// </summary>
public class ApiExceptionFilter : IAsyncActionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;
    private readonly IHostEnvironment _env;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }
    }

    private void HandleException(ActionExecutingContext context, Exception exception)
    {
        int statusCode;
        string message;
        object? data = null;

        switch (exception)
        {
            case ValidationException validationEx:
                statusCode = 400;
                message = validationEx.Message;
                data = new { errors = validationEx.Errors };
                _logger.LogWarning("Validation failed for {Path}: {Message}",
                    context.HttpContext.Request.Path, message);
                break;

            case ClsException clsEx:
                statusCode = clsEx.StatusCode;
                message = clsEx.Message;
                _logger.LogWarning("Domain exception [{StatusCode}] at {Path}: {Message}",
                    statusCode, context.HttpContext.Request.Path, message);
                break;

            default:
                // Phát hiện lỗi kết nối DB từ repository (NpgsqlException, SocketException, ...)
                if (IsTransientDatabaseException(exception))
                {
                    statusCode = 503;
                    message = "Không thể kết nối đến cơ sở dữ liệu. Vui lòng thử lại sau.";
                    _logger.LogError(exception,
                        "Transient database error at {Method} {Path}",
                        context.HttpContext.Request.Method, context.HttpContext.Request.Path);
                }
                else
                {
                    statusCode = 500;
                    message = _env.IsDevelopment()
                        ? exception.Message
                        : "An unexpected error occurred. Please try again later.";
                    _logger.LogError(exception, "Unexpected error at {Method} {Path}",
                        context.HttpContext.Request.Method, context.HttpContext.Request.Path);
                }
                break;
        }

        var response = new
        {
            code = statusCode,
            message,
            data
        };

        context.Result = new ObjectResult(response) { StatusCode = statusCode };
    }

    /// <summary>
    /// Kiểm tra exception chain có chứa lỗi transient database (mất kết nối, timeout) không.
    /// </summary>
    private static bool IsTransientDatabaseException(Exception ex)
    {
        if (ex is DbException) return true;
        if (ex is System.Net.Sockets.SocketException) return true;
        if (ex is TimeoutException) return true;

        // InvalidOperationException wrapping "transient failure"
        if (ex is InvalidOperationException && ex.InnerException is not null)
            return IsTransientDatabaseException(ex.InnerException);

        return false;
    }
}
