using System.Net;
using System.Net.Http.Json;

namespace Prescription.IntegrationTests.Api;

/// <summary>Prescription endpoints and OData over HTTP (02_seed.sql: prescriptions 1 to 4).</summary>
public class PrescriptionsApiTests : ApiTest
{
    private static object CreateRequest(string format = "Structured", string? freeText = null, bool withItem = true) => new
    {
        clientId = 1,
        institutionId = 1,
        format,
        category = "Antimicrobial",
        freeText,
        items = withItem
            ? new object[]
            {
                new
                {
                    medicationId = 5, dose = 1, doseUnitId = 7, frequencyId = 3, startDate = "2026-10-11",
                    durationDays = 7, isContinuousUse = false, administrationRouteId = 1, usageDeadline = "2026-10-21"
                }
            }
            : null
    };

    // ---- X-Professional-Id header ----

    [SkippableTheory]
    [InlineData(null, "Cabeçalho X-Professional-Id não informado.")]
    [InlineData("abc", "Cabeçalho X-Professional-Id inválido.")]
    [InlineData("99", "Profissional 99 não encontrado.")]
    public async Task Header_MissingInvalidOrUnknown_Returns400(string? header, string detail)
    {
        var client = Client();
        if (header is not null)
            client.DefaultRequestHeaders.Add("X-Professional-Id", header);

        var response = await client.GetAsync("/odata/Prescriptions");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(detail, (await Json(response)).GetProperty("detail").GetString());
    }

    // ---- OData ----

    [SkippableFact]
    public async Task OData_ListsOnlyOwnPrescriptions_MostRecentFirst()
    {
        var response = await Client(2).GetAsync("/odata/Prescriptions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await Json(response);
        Assert.True(json.TryGetProperty("@odata.context", out _));
        var rows = json.GetProperty("value").EnumerateArray().ToList();
        Assert.Equal([4, 2], rows.Select(r => r.GetProperty("Id").GetInt64()));
        Assert.Equal("SpecialControl", rows[0].GetProperty("Category").GetString());
        Assert.Equal("Rafael Almeida Rocha", rows[0].GetProperty("ClientName").GetString());
    }

    [SkippableFact]
    public async Task OData_Filter()
    {
        var json = await Json(await Client(2).GetAsync("/odata/Prescriptions?$filter=Category eq 'Antimicrobial'"));

        Assert.Equal([2], json.GetProperty("value").EnumerateArray().Select(r => r.GetProperty("Id").GetInt64()));
    }

    [SkippableFact]
    public async Task OData_OrderBy()
    {
        var json = await Json(await Client(2).GetAsync("/odata/Prescriptions?$orderby=IssuedAt asc"));

        Assert.Equal([2, 4], json.GetProperty("value").EnumerateArray().Select(r => r.GetProperty("Id").GetInt64()));
    }

    [SkippableFact]
    public async Task OData_Select_ReturnsOnlyChosenFields()
    {
        var json = await Json(await Client(2).GetAsync("/odata/Prescriptions?$select=Id,ClientName"));

        var row = json.GetProperty("value")[0];
        Assert.Equal(["Id", "ClientName"], row.EnumerateObject().Select(p => p.Name));
    }

    [SkippableFact]
    public async Task OData_TopSkipAndCount()
    {
        var json = await Json(await Client(2).GetAsync("/odata/Prescriptions?$count=true&$top=1&$skip=1"));

        Assert.Equal(2, json.GetProperty("@odata.count").GetInt64());
        Assert.Equal([2], json.GetProperty("value").EnumerateArray().Select(r => r.GetProperty("Id").GetInt64()));
    }

    [SkippableFact]
    public async Task OData_DatesInBrasiliaTime()
    {
        var json = await Json(await Client(2).GetAsync("/odata/Prescriptions?$filter=Id eq 4"));

        Assert.Equal("2026-10-05T16:20:00-03:00", json.GetProperty("value")[0].GetProperty("IssuedAt").GetString());
    }

    [SkippableFact]
    public async Task OData_TopAboveMax_Returns400()
    {
        var response = await Client(2).GetAsync("/odata/Prescriptions?$top=101");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---- CRUD ----

    [SkippableFact]
    public async Task Get_ReturnsPrescriptionWithEnumsAsText()
    {
        var response = await Client().GetAsync("/api/prescriptions/2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await Json(response);
        Assert.Equal("Structured", json.GetProperty("format").GetString());
        Assert.Equal("Antimicrobial", json.GetProperty("category").GetString());
        Assert.True(json.GetProperty("isIssued").GetBoolean());
        Assert.Equal("2026-10-12", json.GetProperty("items")[0].GetProperty("usageDeadline").GetString());
    }

    [SkippableFact]
    public async Task Get_NotFound_Returns404ProblemDetails()
    {
        var response = await Client().GetAsync("/api/prescriptions/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var json = await Json(response);
        Assert.Equal("Receita 999 não encontrada.", json.GetProperty("detail").GetString());
        Assert.Equal(404, json.GetProperty("status").GetInt32());
    }

    [SkippableFact]
    public async Task Post_Creates_Returns201WithLocation()
    {
        var client = Client(1);

        var response = await client.PostAsJsonAsync("/api/prescriptions", CreateRequest(freeText: "Retornar em 7 dias"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var created = await Json(response);
        Assert.Equal("Amoxicilina 500 mg", created.GetProperty("items")[0].GetProperty("medicationName").GetString());
        Assert.False(created.GetProperty("isIssued").GetBoolean());

        var reloaded = await client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, reloaded.StatusCode);
    }

    [SkippableFact]
    public async Task Post_DomainRule_Returns400WithReason()
    {
        var response = await Client(1).PostAsJsonAsync("/api/prescriptions",
                                                       CreateRequest("Unstructured", "   ", withItem: false));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Receita não estruturada exige texto livre.", (await Json(response)).GetProperty("detail").GetString());
    }

    [SkippableFact]
    public async Task Post_InvalidEnumText_Returns400()
    {
        var response = await Client(1).PostAsJsonAsync("/api/prescriptions", CreateRequest(format: "Invalido"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [SkippableFact]
    public async Task Put_IssuedPrescription_Returns400()
    {
        var response = await Client(1).PutAsJsonAsync("/api/prescriptions/1",
                                                      CreateRequest("Unstructured", "Novo texto", withItem: false));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Receita já emitida (impressa) não pode ser alterada.",
                     (await Json(response)).GetProperty("detail").GetString());
    }

    [SkippableFact]
    public async Task Put_PrescriptionOfOtherProfessional_Returns400()
    {
        var response = await Client(1).PutAsJsonAsync("/api/prescriptions/3",
                                                      CreateRequest("Unstructured", "Novo texto", withItem: false));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Receita pertence a outro profissional.", (await Json(response)).GetProperty("detail").GetString());
    }

    [SkippableFact]
    public async Task Put_Own_Updates()
    {
        var response = await Client(3).PutAsJsonAsync("/api/prescriptions/3",
                                                      CreateRequest("Unstructured", "Losartana 50 mg", withItem: false));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await Json(response);
        Assert.Equal("Unstructured", json.GetProperty("format").GetString());
        Assert.Equal(0, json.GetProperty("items").GetArrayLength());
    }

    [SkippableFact]
    public async Task Delete_Own_Returns204_ThenGet404()
    {
        var client = Client(3);

        var response = await client.DeleteAsync("/api/prescriptions/3");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/prescriptions/3")).StatusCode);
    }

    // ---- Print (ECF.10.04) and issue ----

    [SkippableFact]
    public async Task PrintData_SpecialControl_HasClientAddress()
    {
        var json = await Json(await Client().GetAsync("/api/prescriptions/4/print-data"));

        Assert.Equal("Avenida Paulista", json.GetProperty("client").GetProperty("address").GetProperty("street").GetString());
        Assert.Equal(JsonValueKindNull, json.GetProperty("client").GetProperty("age").ValueKind);
    }

    [SkippableFact]
    public async Task PrintConfirmation_IssuesPrescription()
    {
        var response = await Client(3).PostAsync("/api/prescriptions/3/print-confirmation", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True((await Json(response)).GetProperty("isIssued").GetBoolean());
    }

    // ---- Suggestions ----

    [SkippableFact]
    public async Task SuggestedCategory()
    {
        var json = await Json(await Client().GetAsync("/api/prescriptions/suggested-category?medicationIds=5&medicationIds=14"));

        Assert.Equal("SpecialControl", json.GetProperty("category").GetString());
    }

    private const System.Text.Json.JsonValueKind JsonValueKindNull = System.Text.Json.JsonValueKind.Null;
}
