using System.Net;
using System.Text;

namespace Prescription.Blazor.Tests.Fakes;

/// <summary>Answers every request with a fixed response (or exception) and records the requests.</summary>
public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;

    private FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) => _respond = respond;

    public List<RecordedRequest> Requests { get; } = [];

    public RecordedRequest LastRequest => Requests[^1];

    public static FakeHttpMessageHandler Json(HttpStatusCode status, string json) =>
        new(_ => JsonResponse(status, json));

    /// <summary>Different answer per request (e.g. list, delete, list again).</summary>
    public static FakeHttpMessageHandler Route(Func<HttpRequestMessage, HttpResponseMessage> respond) => new(respond);

    public static HttpResponseMessage JsonResponse(HttpStatusCode status, string json) =>
        new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    public static FakeHttpMessageHandler NoContent() => new(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

    public static FakeHttpMessageHandler Throws(Exception exception) => new(_ => throw exception);

    public HttpClient CreateClient() => new(this) { BaseAddress = new Uri("http://api.test/") };

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                                 CancellationToken cancellationToken)
    {
        // Copied now: the client disposes the request after sending it.
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        var professionalId = request.Headers.TryGetValues("X-Professional-Id", out var values)
            ? values.Single()
            : null;
        Requests.Add(new RecordedRequest(request.Method, request.RequestUri!, body, professionalId));
        return _respond(request);
    }
}

public sealed record RecordedRequest(HttpMethod Method, Uri Uri, string? Body, string? ProfessionalId)
{
    /// <summary>Path and query with the escaping undone, e.g. "/odata/Prescriptions?$top=20".</summary>
    public string PathAndQuery => Uri.UnescapeDataString(Uri.PathAndQuery);
}
