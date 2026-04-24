using CLS.BLL.Common;
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
}
