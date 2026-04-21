namespace CLS.BLL.Common.Exceptions;

/// <summary>
/// Thrown when input data fails business validation rules.
/// Maps to HTTP 400 Bad Request.
///
/// Carries an Errors dictionary for field-level validation messages,
/// matching the API design spec (L2 section 3.3):
/// { "errors": { "fullName": ["Full name is required"], "email": ["Invalid format"] } }
///
/// Example usage:
///   throw new ValidationException("Email", "Email address is already in use.");
///
///   // Or with FluentValidation result:
///   var errors = result.ToDictionary();
///   throw new ValidationException(errors);
/// </summary>
public class ValidationException : ClsException
{
    /// <summary>Field-level validation errors. Key = field name, Value = list of error messages.</summary>
    public IDictionary<string, string[]> Errors { get; }

    /// <summary>Throw with a single field error.</summary>
    public ValidationException(string fieldName, string errorMessage)
        : base("Validation failed.", 400)
    {
        Errors = new Dictionary<string, string[]>
        {
            { fieldName, [errorMessage] }
        };
    }

    /// <summary>Throw with multiple field errors (e.g. from FluentValidation result).</summary>
    public ValidationException(IDictionary<string, string[]> errors)
        : base("Validation failed.", 400)
    {
        Errors = errors;
    }

    /// <summary>Throw with a general message (no field-level breakdown).</summary>
    public ValidationException(string message)
        : base(message, 400)
    {
        Errors = new Dictionary<string, string[]>();
    }
}
