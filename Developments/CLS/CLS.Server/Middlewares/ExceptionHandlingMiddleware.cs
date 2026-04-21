using System.Text.Json;
using CLS.BLL.Common;
using CLS.BLL.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CLS.Server.Middlewares;

/// <summary>
/// Global exception handling middleware — bắt tất cả unhandled exceptions
/// và map về ApiResponse.Fail(...) thống nhất.
///
/// Thứ tự ưu tiên:
///   1. ValidationException  → 400 + errors field
///   2. ClsException (domain) → StatusCode từ exception
///   3. Exception (bất kỳ)   → 500 (không leak stack trace ra Production)
///
/// Implement IMiddleware (DI-based) thay vì convention-based để dễ test
/// và inject dependencies qua constructor.
/// </summary>
public class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed for {Path}: {Message}", context.Request.Path, ex.Message);
            await WriteErrorResponse(context, 400, ex.Message, new { errors = ex.Errors });
        }
        catch (ClsException ex)
        {
            // Bắt tất cả domain exceptions (NotFoundException, ConflictException, v.v.)
            _logger.LogWarning("Domain exception [{StatusCode}] at {Path}: {Message}",
                ex.StatusCode, context.Request.Path, ex.Message);
            await WriteErrorResponse(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            // Unexpected server error — log đầy đủ nhưng KHÔNG trả về chi tiết cho client
            _logger.LogError(ex, "Unexpected error at {Method} {Path}",
                context.Request.Method, context.Request.Path);

            var message = IsDevEnvironment(context)
                ? ex.Message           // Dev: thêm thông tin để debug
                : "An unexpected error occurred. Please try again later.";

            await WriteErrorResponse(context, 500, message);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static async Task WriteErrorResponse(
        HttpContext context,
        int statusCode,
        string message,
        object? data = null)
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

        await context.Response.WriteAsync(json);
    }

    private static bool IsDevEnvironment(HttpContext context)
        => context.RequestServices
               .GetService<IWebHostEnvironment>()
               ?.IsDevelopment() ?? false;
}
