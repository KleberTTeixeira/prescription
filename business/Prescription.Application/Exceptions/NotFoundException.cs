namespace Prescription.Application.Exceptions;

/// <summary>Requested record does not exist. Returned by the API as HTTP 404 with the message.</summary>
public sealed class NotFoundException(string message) : Exception(message);
