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
    /// Ghi activity log inline (await) — giữ đúng DI scope.
    /// Lỗi ghi log được swallow, không ảnh hưởng response.
    /// </summary>
    internal static async Task LogActivityAsync(
        this ControllerBase controller,
        IActivityLogService activityLogService,
        string actionType,
        string description)
    {
        var userId = controller.GetCurrentUserId();
        if (userId <= 0) return;

        try
        {
            await activityLogService.LogAsync(userId, actionType, description);
        }
        catch
        {
            // Activity log failure must never break the main flow
        }
    }

    /// <summary>Lấy userId từ JWT claims — dùng chung cho mọi Controller.</summary>
    internal static int GetCurrentUserId(this ControllerBase controller)
    {
        var claim = controller.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                    ?? controller.User.FindFirst("sub");
        return claim is not null && int.TryParse(claim.Value, out var id) ? id : 0;
    }
}
