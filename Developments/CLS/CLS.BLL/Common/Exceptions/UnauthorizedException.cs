namespace CLS.BLL.Common.Exceptions;

/// <summary>
/// Thrown when an operation requires authentication but no valid token is provided.
/// Maps to HTTP 401 Unauthorized.
///
/// Note: In most cases, the JWT middleware handles 401 automatically.
/// Use this exception when authentication logic is handled within Service layer
/// (e.g., manual token validation, refresh token expired).
///
/// Example usage:
///   throw new UnauthorizedException("Your session has expired. Please log in again.");
/// </summary>
public class UnauthorizedException : ClsException
{
    public UnauthorizedException(string message = "Authentication required. Please provide a valid Bearer token.")
        : base(message, 401) { }
}
