namespace Prescription.Domain.Exceptions;

/// <summary>
/// Business rule violation. The message (in Portuguese) explains the reason
/// and is returned by the API as HTTP 400.
/// </summary>
public sealed class DomainException(string message) : Exception(message);
