using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Prescription.Blazor.Services;

/// <summary>
/// Common part of the Web API clients: sends the chosen professional in the X-Professional-Id header and turns
/// errors into <see cref="ApiException"/>. The header is added here and not in a DelegatingHandler because in
/// Blazor Server the HttpClientFactory handlers run in another scope and do not see the session professional.
/// </summary>
public abstract class ApiClientBase(HttpClient httpClient, ILoggedProfessionalStore loggedProfessional)
{
    public const string ProfessionalHeader = "X-Professional-Id";

    /// <summary>Same JSON as the API: camelCase and enums as text.</summary>
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected async Task<T> SendAsync<T>(HttpMethod method, string url, object? body = null,
                                         CancellationToken cancellationToken = default)
    {
        using var response = await SendCoreAsync(method, url, body, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
               ?? throw new ApiException(ApiException.Unexpected);
    }

    protected async Task SendAsync(HttpMethod method, string url, object? body = null,
                                   CancellationToken cancellationToken = default)
    {
        using var response = await SendCoreAsync(method, url, body, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendCoreAsync(HttpMethod method, string url, object? body,
                                                          CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, url);
        if (loggedProfessional.Current is { } professional)
        {
            request.Headers.Add(ProfessionalHeader, professional.Id.ToString());
        }
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, body.GetType(), options: JsonOptions);
        }

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw new ApiException(ApiException.ConnectionFailed, exception);
        }

        if (!response.IsSuccessStatusCode)
        {
            using (response)
            {
                throw await ApiException.FromResponseAsync(response, cancellationToken);
            }
        }
        return response;
    }
}
