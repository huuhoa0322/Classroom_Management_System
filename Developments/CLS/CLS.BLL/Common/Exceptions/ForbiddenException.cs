namespace CLS.BLL.Common.Exceptions;

/// <summary>
/// Thrown when an authenticated user attempts an action they do not have permission for.
/// Maps to HTTP 403 Forbidden.
///
/// Distinction from UnauthorizedException:
///   - 401: User is NOT authenticated (no token / invalid token)
///   - 403: User IS authenticated but lacks the required role/permission
///
/// Example usage in Service:
///   if (currentUser.Role != "Admin")
///       throw new ForbiddenException("Only admins can perform this action.");
/// </summary>
public class ForbiddenException : ClsException
{
    public ForbiddenException(string message = "You do not have permission to perform this action.")
        : base(message, 403) { }
}
