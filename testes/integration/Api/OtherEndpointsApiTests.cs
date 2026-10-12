using System.Net;
using System.Net.Http.Json;

namespace Prescription.IntegrationTests.Api;

/// <summary>Medications, templates, favorites, support lists, Swagger, health check and CORS over HTTP.</summary>
public class OtherEndpointsApiTests : ApiTest
{
    /// <summary>Also proves the API reads the connection string from appsettings.Development.json.</summary>
    [SkippableFact]
    public async Task Health_ReturnsHealthy()
    {
        var response = await Client(shareTransaction: false).GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [SkippableFact]
    public async Task Swagger_DocumentsTheProfessionalHeader()
    {
        var response = await Client().GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("X-Professional-Id", content);
        Assert.Contains("/api/prescriptions", content);
        Assert.Contains("/odata/Prescriptions", content);
    }

    [SkippableFact]
    public async Task Cors_AllowsAngularOrigin()
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/professionals");
        request.Headers.Add("Origin", "http://localhost:4200");
        request.Headers.Add("Access-Control-Request-Method", "GET");
        request.Headers.Add("Access-Control-Request-Headers", "X-Professional-Id");

        var response = await Client().SendAsync(request);

        Assert.Equal("http://localhost:4200", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    [SkippableFact]
    public async Task SupportLists_ReturnSeedData()
    {
        var client = Client();

        Assert.Equal(4, (await Json(await client.GetAsync("/api/professionals"))).GetArrayLength());
        Assert.Equal(6, (await Json(await client.GetAsync("/api/clients"))).GetArrayLength());
        Assert.Equal(4, (await Json(await client.GetAsync("/api/institutions"))).GetArrayLength());
        Assert.Equal(10, (await Json(await client.GetAsync("/api/pharmaceutical-forms"))).GetArrayLength());
        Assert.Equal(10, (await Json(await client.GetAsync("/api/administration-routes"))).GetArrayLength());
        Assert.Equal(10, (await Json(await client.GetAsync("/api/dose-units"))).GetArrayLength());
        Assert.Equal(8, (await Json(await client.GetAsync("/api/frequencies"))).GetArrayLength());
        Assert.Equal(4, (await Json(await client.GetAsync("/api/validity-periods"))).GetArrayLength());

        var joao = (await Json(await client.GetAsync("/api/clients"))).EnumerateArray()
                                                                       .Single(c => c.GetProperty("id").GetInt64() == 1);
        Assert.Equal("Male", joao.GetProperty("sex").GetString());
        Assert.Equal("1980-03-15", joao.GetProperty("birthDate").GetString());
    }

    [SkippableFact]
    public async Task Medications_SearchAndUsageDeadline()
    {
        var client = Client();

        Assert.Equal(2, (await Json(await client.GetAsync("/api/medications?search=sodica"))).GetArrayLength());

        var deadline = await Json(await client.GetAsync("/api/medications/5/usage-deadline?issueDate=2026-10-05"));
        Assert.Equal("2026-10-15", deadline.GetProperty("usageDeadline").GetString());

        var noLimit = await Json(await client.GetAsync("/api/medications/1/usage-deadline?issueDate=2026-10-05"));
        Assert.Equal(System.Text.Json.JsonValueKind.Null, noLimit.GetProperty("usageDeadline").ValueKind);

        Assert.Equal(HttpStatusCode.NotFound,
                     (await client.GetAsync("/api/medications/999/usage-deadline?issueDate=2026-10-05")).StatusCode);
    }

    [SkippableFact]
    public async Task Templates_ListCreateDuplicate()
    {
        var client = Client(1);

        Assert.Equal(2, (await Json(await client.GetAsync("/api/templates"))).GetArrayLength());

        var created = await client.PostAsJsonAsync("/api/templates", new { title = "Repouso", content = "Repouso por 2 dias." });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var duplicate = await client.PostAsJsonAsync("/api/templates", new { title = "repouso", content = "x" });
        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
        Assert.Equal("Já existe texto padrão com o título 'repouso'.",
                     (await Json(duplicate)).GetProperty("detail").GetString());
    }

    [SkippableFact]
    public async Task Templates_RequireHeader()
    {
        Assert.Equal(HttpStatusCode.BadRequest, (await Client().GetAsync("/api/templates")).StatusCode);
    }

    [SkippableFact]
    public async Task Favorites_AddListRemove()
    {
        var client = Client(1);

        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsync("/api/favorites/3", null)).StatusCode);
        var list = await Json(await client.GetAsync("/api/favorites"));
        Assert.Contains(list.EnumerateArray(), m => m.GetProperty("id").GetInt64() == 3);

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync("/api/favorites/3")).StatusCode);
        list = await Json(await client.GetAsync("/api/favorites"));
        Assert.DoesNotContain(list.EnumerateArray(), m => m.GetProperty("id").GetInt64() == 3);
    }
}
