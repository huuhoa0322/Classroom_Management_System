using CLS.BLL.Common;
using CLS.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CLS.Server.Controllers;

internal static class ControllerResultExtensions
{
    public static IActionResult ToOkResponse<T>(
        this ControllerBase controller,
        ServiceResult<T> result,
        string successMessage)
    {
        if (!result.IsSuccess)
            return controller.ToErrorResponse(result);

        return controller.Ok(ApiResponse<T>.Success(result.Value!, successMessage));
    }

    public static IActionResult ToCreatedAtActionResponse<T>(
        this ControllerBase controller,
        ServiceResult<T> result,
        string actionName,
        Func<T, object> routeValuesFactory,
        string successMessage)
    {
        if (!result.IsSuccess)
            return controller.ToErrorResponse(result);

        var value = result.Value!;
        return controller.CreatedAtAction(
            actionName,
            routeValuesFactory(value),
            ApiResponse<T>.Created(value, successMessage));
    }

    private static IActionResult ToErrorResponse<T>(
        this ControllerBase controller,
        ServiceResult<T> result)
        => controller.StatusCode(
            result.StatusCode,
            ApiResponse.Fail(result.Message, result.StatusCode, result.ErrorData));

    // ── Activity Logging Helper ─────────────────────────────────────────────

    /// <summary>
    /// Fire-and-forget ghi activity log sau khi action thành công.
    /// Không block response — lỗi ghi log chỉ được log warning, không ảnh hưởng user.
    /// </summary>
    internal static void LogActivity(
        this ControllerBase controller,
        IActivityLogService activityLogService,
        string actionType,
        string description)
    {
        var userId = GetCurrentUserId(controller);
        if (userId <= 0) return;

        // Fire-and-forget — không await
        _ = Task.Run(async () =>
        {
            try
            {
                await activityLogService.LogAsync(userId, actionType, description);
            }
            catch
            {
                // Swallow — activity log failure must never break the main flow
            }
        });
    }

    private static int GetCurrentUserId(ControllerBase controller)
    {
        var claim = controller.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                    ?? controller.User.FindFirst("sub");
        return claim is not null && int.TryParse(claim.Value, out var id) ? id : 0;
    }
}
