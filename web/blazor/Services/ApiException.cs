using System.Net.Http.Json;

namespace Prescription.Blazor.Services;

/// <summary>Error from the Web API, with the message to show the user (Portuguese).</summary>
public sealed class ApiException(string message, Exception? innerException = null) : Exception(message, innerException)
{
    public const string ConnectionFailed = "Não foi possível conectar ao servidor.";
    public const string Unexpected = "Ocorreu um erro inesperado.";

    /// <summary>The API answers errors as ProblemDetails (RFC 7807) with the reason in "detail".</summary>
    public static async Task<ApiException> FromResponseAsync(HttpResponseMessage response,
                                                             CancellationToken cancellationToken)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsBody>(cancellationToken);
            if (!string.IsNullOrWhiteSpace(problem?.Detail))
            {
                return new ApiException(problem.Detail);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Body is not ProblemDetails: fall back to the generic message.
        }
        return new ApiException(Unexpected);
    }

    private sealed record ProblemDetailsBody(string? Detail);
}
