using System.Net.Http.Json;
using System.Text.Json;

namespace Prescription.IntegrationTests.Api;

/// <summary>Base of the HTTP tests: one in-memory API per test (transaction rolled back at the end).</summary>
public abstract class ApiTest : IDisposable
{
    private static readonly string? ConnectionString = Environment.GetEnvironmentVariable(DatabaseTest.ConnectionVariable);

    private ApiFactory? _factory;

    /// <param name="shareTransaction">false only for read-only checks that must use the real configuration (/health).</param>
    protected HttpClient Client(uint? professionalId = null, bool shareTransaction = true)
    {
        Skip.If(string.IsNullOrWhiteSpace(ConnectionString), $"Variável {DatabaseTest.ConnectionVariable} não definida.");

        _factory ??= new ApiFactory(ConnectionString!, shareTransaction);
        var client = _factory.CreateClient();
        if (professionalId is not null)
            client.DefaultRequestHeaders.Add("X-Professional-Id", professionalId.ToString());
        return client;
    }

    protected static async Task<JsonElement> Json(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();

    public void Dispose()
    {
        _factory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
