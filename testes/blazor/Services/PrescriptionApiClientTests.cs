using System.Net;
using System.Text.Json;
using Prescription.Blazor.Models;
using Prescription.Blazor.Services;
using Prescription.Blazor.Tests.Fakes;

namespace Prescription.Blazor.Tests.Services;

public class PrescriptionApiClientTests
{
    private const string PrescriptionJson =
        """
        { "id": 4, "clientId": 3, "clientName": "Carla Dias", "professionalId": 1, "professionalName": "Ana Souza",
          "institutionId": 1, "institutionName": "Clínica Central", "format": "Structured",
          "category": "Antimicrobial", "freeText": null, "issuedAt": "2026-10-05T16:20:00", "printedAt": null,
          "isIssued": false,
          "items": [{ "id": 7, "medicationId": 5, "medicationName": "Amoxicilina 500 mg", "pharmaceuticalFormId": 1,
                      "pharmaceuticalFormName": "Cápsula", "dose": 1.5, "doseUnitId": 2, "doseUnitAbbreviation": "cáps",
                      "frequencyId": 3, "frequencyName": "8 em 8 horas", "startDate": "2026-10-05",
                      "durationDays": 7, "isContinuousUse": false, "administrationRouteId": 1,
                      "administrationRouteName": "Oral", "usageDeadline": "2026-10-15", "notes": null }] }
        """;

    private static PrescriptionApiClient Create(FakeHttpMessageHandler handler) =>
        new(handler.CreateClient(), new FakeLoggedProfessionalStore(TestData.Ana).Loaded());

    private static PrescriptionListQuery Query(string clientName = "",
                                               PrescriptionSortField field = PrescriptionSortField.IssuedAt,
                                               PrescriptionSortDirection direction = PrescriptionSortDirection.Desc) =>
        new(clientName, new PrescriptionSort(field, direction), Skip: 40, Top: 20);

    private const string ODataJson =
        """
        { "@odata.context": "http://api.test/odata/$metadata#Prescriptions", "@odata.count": 41,
          "value": [{ "Id": 4, "ClientId": 3, "ClientName": "Carla Dias", "ProfessionalId": 1,
                      "ProfessionalName": "Ana Souza", "InstitutionId": 1, "InstitutionName": "Clínica Central",
                      "Format": "Structured", "Category": "Antimicrobial",
                      "IssuedAt": "2026-10-05T16:20:00-03:00", "PrintedAt": null, "IsIssued": false,
                      "ItemCount": 2 }] }
        """;

    [Fact]
    public async Task List_BuildsODataQueryWithProfessionalHeader()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.OK, ODataJson);

        await Create(handler).ListAsync(Query());

        Assert.Equal("/odata/Prescriptions?$count=true&$orderby=IssuedAt desc,Id desc&$top=20&$skip=40",
                     handler.LastRequest.PathAndQuery);
        Assert.Equal("1", handler.LastRequest.ProfessionalId);
    }

    [Theory]
    [InlineData(PrescriptionSortField.Id, PrescriptionSortDirection.Asc, "Id asc")]
    [InlineData(PrescriptionSortField.ClientName, PrescriptionSortDirection.Asc, "ClientName asc,Id asc")]
    [InlineData(PrescriptionSortField.Format, PrescriptionSortDirection.Desc, "Format desc,Id desc")]
    [InlineData(PrescriptionSortField.Category, PrescriptionSortDirection.Asc, "Category asc,Id asc")]
    [InlineData(PrescriptionSortField.ItemCount, PrescriptionSortDirection.Desc, "ItemCount desc,Id desc")]
    [InlineData(PrescriptionSortField.IsIssued, PrescriptionSortDirection.Asc, "IsIssued asc,Id asc")]
    public async Task List_OrderByWithIdAsTieBreaker(PrescriptionSortField field, PrescriptionSortDirection direction,
                                                     string orderBy)
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.OK, ODataJson);

        await Create(handler).ListAsync(Query(field: field, direction: direction));

        Assert.Contains($"$orderby={orderBy}&", handler.LastRequest.PathAndQuery);
    }

    [Fact]
    public async Task List_FilterByClientName_EscapesQuotes()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.OK, ODataJson);

        await Create(handler).ListAsync(Query("  D'Ávila  "));

        Assert.EndsWith("&$filter=contains(ClientName,'D''Ávila')", handler.LastRequest.PathAndQuery);
    }

    [Fact]
    public async Task List_BlankClientName_NoFilter()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.OK, ODataJson);

        await Create(handler).ListAsync(Query("   "));

        Assert.DoesNotContain("$filter", handler.LastRequest.PathAndQuery);
    }

    [Fact]
    public async Task List_ReadsRowsTotalAndKeepsBrasiliaTime()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.OK, ODataJson);

        var page = await Create(handler).ListAsync(Query());

        Assert.Equal(41, page.Total);
        var row = Assert.Single(page.Rows);
        Assert.Equal(new PrescriptionListItem(4, "Carla Dias", "Clínica Central", PrescriptionFormat.Structured,
                                              PrescriptionCategory.Antimicrobial, new DateTime(2026, 10, 5, 16, 20, 0),
                                              false, 2), row);
    }

    [Fact]
    public async Task Get_ReadsPrescriptionAndItems()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.OK, PrescriptionJson);

        var prescription = await Create(handler).GetAsync(4);

        Assert.Equal("/api/prescriptions/4", handler.LastRequest.PathAndQuery);
        Assert.Equal(PrescriptionFormat.Structured, prescription.Format);
        Assert.Equal(new DateTime(2026, 10, 5, 16, 20, 0), prescription.IssuedAt);
        var item = Assert.Single(prescription.Items);
        Assert.Equal(1.5m, item.Dose);
        Assert.Equal(new DateOnly(2026, 10, 15), item.UsageDeadline);
    }

    [Fact]
    public async Task Create_PostsRequestWithEnumsAsTextAndDates()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.Created, PrescriptionJson);
        var request = new PrescriptionRequest(3, 1, PrescriptionFormat.Structured, PrescriptionCategory.SpecialControl,
            null,
            [new PrescriptionItemRequest(null, "Fórmula manipulada", 2, 10m, 1, 3, new DateOnly(2026, 10, 5), 30,
                                         false, 1, null, "Observação")]);

        await Create(handler).CreateAsync(request);

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("/api/prescriptions", handler.LastRequest.PathAndQuery);
        using var body = JsonDocument.Parse(handler.LastRequest.Body!);
        Assert.Equal("Structured", body.RootElement.GetProperty("format").GetString());
        Assert.Equal("SpecialControl", body.RootElement.GetProperty("category").GetString());
        var item = body.RootElement.GetProperty("items")[0];
        Assert.Equal("2026-10-05", item.GetProperty("startDate").GetString());
        Assert.Equal(JsonValueKind.Null, item.GetProperty("medicationId").ValueKind);
        Assert.Equal("Fórmula manipulada", item.GetProperty("medicationName").GetString());
    }

    [Fact]
    public async Task Update_PutsToPrescription()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.OK, PrescriptionJson);
        var request = new PrescriptionRequest(3, 1, PrescriptionFormat.Unstructured, PrescriptionCategory.Common,
                                              "Texto livre", []);

        await Create(handler).UpdateAsync(4, request);

        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.Equal("/api/prescriptions/4", handler.LastRequest.PathAndQuery);
        Assert.Contains("\"freeText\":\"Texto livre\"", handler.LastRequest.Body);
    }

    [Fact]
    public async Task Delete_SendsDelete()
    {
        var handler = FakeHttpMessageHandler.NoContent();

        await Create(handler).DeleteAsync(4);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal("/api/prescriptions/4", handler.LastRequest.PathAndQuery);
    }

    [Fact]
    public async Task Delete_OtherProfessional_ShowsApiReason()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.BadRequest,
                                                  """{ "detail": "Receita pertence a outro profissional." }""");

        var exception = await Assert.ThrowsAsync<ApiException>(() => Create(handler).DeleteAsync(4));

        Assert.Equal("Receita pertence a outro profissional.", exception.Message);
    }

    [Fact]
    public async Task GetPrintData_ReadsClientProfessionalAndInstitution()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.OK,
            """
            { "id": 4, "format": "Unstructured", "category": "Antimicrobial", "freeText": "Texto",
              "issuedAt": "2026-10-05T16:20:00", "printedAt": null,
              "client": { "fullName": "Carla Dias", "address": null, "age": 36, "sex": "Female" },
              "professional": { "id": 1, "fullName": "Ana Souza", "council": "CRM", "councilNumber": "12345",
                                "councilState": "SP" },
              "institution": { "id": 1, "name": "Clínica Central", "cnes": "1234567", "phone": "1133334444",
                               "address": { "street": "Rua B", "number": "20", "complement": "Sala 3",
                                            "district": "Centro", "city": "São Paulo", "state": "SP",
                                            "zipCode": "01002000" } },
              "items": [] }
            """);

        var data = await Create(handler).GetPrintDataAsync(4);

        Assert.Equal("/api/prescriptions/4/print-data", handler.LastRequest.PathAndQuery);
        Assert.Equal(new PrintClient("Carla Dias", null, 36, Sex.Female), data.Client);
        Assert.Equal(TestData.Ana, data.Professional);
        Assert.Equal("1234567", data.Institution.Cnes);
    }

    [Fact]
    public async Task ConfirmPrint_PostsWithoutBody()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.OK, PrescriptionJson);

        await Create(handler).ConfirmPrintAsync(4);

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("/api/prescriptions/4/print-confirmation", handler.LastRequest.PathAndQuery);
        Assert.Null(handler.LastRequest.Body);
    }

    [Fact]
    public async Task SuggestCategory_SendsMedicationIds()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.OK, """{ "category": "SpecialControl" }""");

        var category = await Create(handler).SuggestCategoryAsync([5, 16]);

        Assert.Equal("/api/prescriptions/suggested-category?medicationIds=5&medicationIds=16",
                     handler.LastRequest.PathAndQuery);
        Assert.Equal(PrescriptionCategory.SpecialControl, category);
    }

    [Fact]
    public async Task SuggestCategory_NoMedications_NoQuery()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.OK, """{ "category": "Common" }""");

        var category = await Create(handler).SuggestCategoryAsync([]);

        Assert.Equal("/api/prescriptions/suggested-category", handler.LastRequest.PathAndQuery);
        Assert.Equal(PrescriptionCategory.Common, category);
    }
}
