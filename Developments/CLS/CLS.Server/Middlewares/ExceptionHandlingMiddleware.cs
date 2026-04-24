using System.Text.Json;
using CLS.BLL.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace CLS.Server.Middlewares;

/// <summary>
/// Global exception handler — bắt tất cả unhandled exceptions
/// và map về ApiResponse format thống nhất.
///
/// Sử dụng IExceptionHandler (.NET 8+) thay vì IMiddleware để:
/// - Framework xử lý exception ở tầng infrastructure (không bị VS break)
/// - Tích hợp tốt hơn với ASP.NET Core pipeline
/// - VS debugger không đánh dấu "User-Unhandled" gây gián đoạn
///
/// Thứ tự ưu tiên:
///   1. ValidationException  → 400 + errors field
///   2. ClsException (domain) → StatusCode từ exception
///   3. Exception (bất kỳ)   → 500 (không leak stack trace ra Production)
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken ct)
    {
        switch (exception)
        {
            case ValidationException validationEx:
                _logger.LogWarning("Validation failed for {Path}: {Message}",
                    context.Request.Path, validationEx.Message);
                await WriteErrorResponse(context, 400, validationEx.Message,
                    new { errors = validationEx.Errors }, ct);
                return true;

            case ClsException clsEx:
                _logger.LogWarning("Domain exception [{StatusCode}] at {Path}: {Message}",
                    clsEx.StatusCode, context.Request.Path, clsEx.Message);
                await WriteErrorResponse(context, clsEx.StatusCode, clsEx.Message, ct: ct);
                return true;

            default:
                _logger.LogError(exception, "Unexpected error at {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                var message = _env.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred. Please try again later.";

                await WriteErrorResponse(context, 500, message, ct: ct);
                return true;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static async Task WriteErrorResponse(
        HttpContext context,
        int statusCode,
        string message,
        object? data = null,
        CancellationToken ct = default)
    {
        context.Response.StatusCode  = statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            code    = statusCode,
            message,
            data
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json, ct);
    }
}
