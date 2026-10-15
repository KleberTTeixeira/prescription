using System.Net;

namespace Prescription.Blazor.Tests.Fakes;

/// <summary>Fake Web API: one answer per "METHOD path" (query string ignored); records every request.</summary>
public sealed class FakeApi
{
    private readonly Dictionary<string, (HttpStatusCode Status, string Json)> _routes = [];

    public FakeApi() =>
        Handler = FakeHttpMessageHandler.Route(request =>
        {
            var key = $"{request.Method} {request.RequestUri!.AbsolutePath}";
            return _routes.TryGetValue(key, out var route)
                ? FakeHttpMessageHandler.JsonResponse(route.Status, route.Json)
                : FakeHttpMessageHandler.JsonResponse(HttpStatusCode.NotFound,
                                                      $$"""{ "detail": "Rota não configurada no teste: {{key}}" }""");
        });

    public FakeHttpMessageHandler Handler { get; }

    public IEnumerable<RecordedRequest> Requests => Handler.Requests;

    public FakeApi On(HttpMethod method, string path, string json, HttpStatusCode status = HttpStatusCode.OK)
    {
        _routes[$"{method} {path}"] = (status, json);
        return this;
    }

    public FakeApi Get(string path, string json, HttpStatusCode status = HttpStatusCode.OK) =>
        On(HttpMethod.Get, path, json, status);

    public IReadOnlyList<RecordedRequest> To(HttpMethod method, string path) =>
        Requests.Where(r => r.Method == method && r.Uri.AbsolutePath == path).ToList();
}
