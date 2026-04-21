namespace CLS.BLL.Common.Exceptions;

/// <summary>
/// Thrown when a requested resource does not exist in the system.
/// Maps to HTTP 404 Not Found.
///
/// Example usage in Service:
///   var student = await _repo.GetByIdAsync(id)
///       ?? throw new NotFoundException($"Student with ID {id} not found.");
/// </summary>
public class NotFoundException : ClsException
{
    public NotFoundException(string message) : base(message, 404) { }

    /// <summary>Convenience constructor — tự format message theo entity name và id.</summary>
    public NotFoundException(string entityName, object id)
        : base($"{entityName} with ID {id} was not found.", 404) { }
}
