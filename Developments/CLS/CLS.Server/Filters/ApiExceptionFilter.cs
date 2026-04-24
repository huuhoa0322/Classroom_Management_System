using CLS.BLL.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace CLS.Server.Filters;

/// <summary>
/// Global MVC Exception Filter — bắt tất cả exceptions trong controller pipeline
/// và trả về ApiResponse format thống nhất.
///
/// Vì filter chạy trong MVC pipeline (user code), Visual Studio sẽ coi exception
/// là "handled" và KHÔNG hiển thị dialog "Exception User-Unhandled" gây gián đoạn.
///
/// Thứ tự ưu tiên:
///   1. ValidationException  → 400 + errors field
///   2. ClsException (domain) → StatusCode từ exception (401, 404, 409, v.v.)
///   3. Exception (bất kỳ)   → 500 (không leak stack trace ra Production)
/// </summary>
public class ApiExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;
    private readonly IHostEnvironment _env;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public void OnException(ExceptionContext context)
    {
        int statusCode;
        string message;
        object? data = null;

        switch (context.Exception)
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
                statusCode = 500;
                message = _env.IsDevelopment()
                    ? context.Exception.Message
                    : "An unexpected error occurred. Please try again later.";
                _logger.LogError(context.Exception, "Unexpected error at {Method} {Path}",
                    context.HttpContext.Request.Method, context.HttpContext.Request.Path);
                break;
        }

        var response = new
        {
            code = statusCode,
            message,
            data
        };

        context.Result = new ObjectResult(response) { StatusCode = statusCode };
        context.ExceptionHandled = true; // ← Đánh dấu đã xử lý → VS không break
    }
}
