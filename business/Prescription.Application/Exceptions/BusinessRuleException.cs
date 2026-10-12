namespace Prescription.Application.Exceptions;

/// <summary>
/// Invalid request detected by the application (e.g. referenced record not found, record of another
/// professional). Returned by the API as HTTP 400 with the message.
/// </summary>
public sealed class BusinessRuleException(string message) : Exception(message);
