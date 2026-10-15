using System.Net;
using Prescription.Blazor.Models;
using Prescription.Blazor.Services;
using Prescription.Blazor.Tests.Fakes;

namespace Prescription.Blazor.Tests.Services;

public class CatalogApiClientTests
{
    private static CatalogApiClient Create(FakeHttpMessageHandler handler, Professional? professional = null) =>
        new(handler.CreateClient(), new FakeLoggedProfessionalStore(professional).Loaded());

    [Fact]
    public async Task GetProfessionals_CallsApiAndReadsList()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.OK, TestData.ProfessionalsJson);

        var professionals = await Create(handler).GetProfessionalsAsync();

        Assert.Equal("http://api.test/api/professionals", handler.LastRequest.Uri.ToString());
        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal([TestData.Ana, TestData.Bruno], professionals);
    }

    [Fact]
    public async Task WithoutProfessional_NoProfessionalHeader()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.OK, TestData.ProfessionalsJson);

        await Create(handler).GetProfessionalsAsync();

        Assert.Null(handler.LastRequest.ProfessionalId);
    }

    [Fact]
    public async Task GetClients_ReadsDatesSexAndAddress()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.OK,
            """
            [{ "id": 3, "fullName": "Carla Dias", "birthDate": "1990-05-20", "sex": "Female",
               "address": { "street": "Rua A", "number": "10", "complement": null, "district": "Centro",
                            "city": "São Paulo", "state": "SP", "zipCode": "01001000" } }]
            """);

        var client = Assert.Single(await Create(handler, TestData.Ana).GetClientsAsync());

        Assert.Equal("/api/clients", handler.LastRequest.PathAndQuery);
        Assert.Equal("1", handler.LastRequest.ProfessionalId);
        Assert.Equal(new DateOnly(1990, 5, 20), client.BirthDate);
        Assert.Equal(Sex.Female, client.Sex);
        Assert.Equal("Rua A, 10, Centro, São Paulo/SP, CEP 01001-000", client.Address.ToString());
    }

    [Theory]
    [InlineData("institutions")]
    [InlineData("pharmaceutical-forms")]
    [InlineData("administration-routes")]
    [InlineData("dose-units")]
    [InlineData("frequencies")]
    [InlineData("validity-periods")]
    public async Task SupportLists_CallTheirEndpoints(string endpoint)
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.OK, "[]");
        var catalog = Create(handler);

        Task call = endpoint switch
        {
            "institutions" => catalog.GetInstitutionsAsync(),
            "pharmaceutical-forms" => catalog.GetPharmaceuticalFormsAsync(),
            "administration-routes" => catalog.GetAdministrationRoutesAsync(),
            "dose-units" => catalog.GetDoseUnitsAsync(),
            "frequencies" => catalog.GetFrequenciesAsync(),
            _ => catalog.GetValidityPeriodsAsync()
        };
        await call;

        Assert.Equal($"/api/{endpoint}", handler.LastRequest.PathAndQuery);
    }

    [Fact]
    public async Task GetValidityPeriods_DaysCanBeEmpty()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.OK,
            """[{ "id": 1, "name": "Sem prazo", "days": null }, { "id": 2, "name": "Antimicrobiano - 10 dias", "days": 10 }]""");

        var periods = await Create(handler).GetValidityPeriodsAsync();

        Assert.Equal([new ValidityPeriod(1, "Sem prazo", null), new ValidityPeriod(2, "Antimicrobiano - 10 dias", 10)],
                     periods);
    }

    [Fact]
    public async Task ProblemDetails_UsesDetailAsMessage()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.BadRequest,
                                                  """{ "status": 400, "detail": "Motivo do erro." }""");

        var exception = await Assert.ThrowsAsync<ApiException>(() => Create(handler).GetProfessionalsAsync());

        Assert.Equal("Motivo do erro.", exception.Message);
    }

    [Fact]
    public async Task ErrorWithoutDetail_UsesGenericMessage()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.InternalServerError, "não é json");

        var exception = await Assert.ThrowsAsync<ApiException>(() => Create(handler).GetProfessionalsAsync());

        Assert.Equal("Ocorreu um erro inesperado.", exception.Message);
    }

    [Fact]
    public async Task ServerUnreachable_UsesConnectionMessage()
    {
        var handler = FakeHttpMessageHandler.Throws(new HttpRequestException("recusado"));

        var exception = await Assert.ThrowsAsync<ApiException>(() => Create(handler).GetProfessionalsAsync());

        Assert.Equal("Não foi possível conectar ao servidor.", exception.Message);
    }
}
