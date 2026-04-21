namespace CLS.BLL.Common.Exceptions;

/// <summary>
/// Thrown when a business rule conflict is detected.
/// Maps to HTTP 409 Conflict.
///
/// Primary use case in CLS: scheduling conflict (teacher/room unavailable).
///
/// Example usage in Service:
///   throw new ConflictException("Scheduling conflict: Teacher is unavailable at the requested time.");
/// </summary>
public class ConflictException : ClsException
{
    public ConflictException(string message) : base(message, 409) { }
}
