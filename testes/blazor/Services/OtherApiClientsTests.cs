using System.Net;
using Prescription.Blazor.Models;
using Prescription.Blazor.Services;
using Prescription.Blazor.Tests.Fakes;

namespace Prescription.Blazor.Tests.Services;

/// <summary>Medications, standard texts and favorites.</summary>
public class OtherApiClientsTests
{
    private const string MedicationsJson =
        """
        [{ "id": 5, "name": "Amoxicilina", "activeIngredient": "amoxicilina", "concentration": "500 mg",
           "displayName": "Amoxicilina 500 mg", "pharmaceuticalFormId": 1, "pharmaceuticalFormName": "Cápsula",
           "validityPeriodDays": 10, "isControlled": false, "isAntimicrobial": true }]
        """;

    private const string TemplateJson =
        """{ "id": 2, "title": "Gripe", "content": "Repouso.", "createdAt": "2026-10-01T09:30:00", "updatedAt": null }""";

    private static FakeLoggedProfessionalStore Store() => new FakeLoggedProfessionalStore(TestData.Bruno).Loaded();

    [Fact]
    public async Task Medications_Search_EscapesTermAndReadsList()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.OK, MedicationsJson);

        var medication = Assert.Single(await new MedicationApiClient(handler.CreateClient(), Store())
                                           .SearchAsync("amox & cia"));

        Assert.Equal("/api/medications?search=amox%20%26%20cia", handler.LastRequest.Uri.PathAndQuery);
        Assert.Equal("2", handler.LastRequest.ProfessionalId);
        Assert.Equal(10u, medication.ValidityPeriodDays);
        Assert.True(medication.IsAntimicrobial);
    }

    [Fact]
    public async Task Medications_UsageDeadline_SendsIssueDate()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.OK, """{ "usageDeadline": "2026-10-15" }""");

        var deadline = await new MedicationApiClient(handler.CreateClient(), Store())
            .SuggestUsageDeadlineAsync(5, new DateOnly(2026, 10, 5));

        Assert.Equal("/api/medications/5/usage-deadline?issueDate=2026-10-05", handler.LastRequest.PathAndQuery);
        Assert.Equal(new DateOnly(2026, 10, 15), deadline);
    }

    [Fact]
    public async Task Medications_UsageDeadline_NoLimit_IsNull()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.OK, """{ "usageDeadline": null }""");

        var deadline = await new MedicationApiClient(handler.CreateClient(), Store())
            .SuggestUsageDeadlineAsync(1, new DateOnly(2026, 10, 5));

        Assert.Null(deadline);
    }

    [Fact]
    public async Task Templates_List()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.OK, $"[{TemplateJson}]");

        var template = Assert.Single(await new TemplateApiClient(handler.CreateClient(), Store()).ListAsync());

        Assert.Equal("/api/templates", handler.LastRequest.PathAndQuery);
        Assert.Equal("2", handler.LastRequest.ProfessionalId);
        Assert.Equal(new Template(2, "Gripe", "Repouso.", new DateTime(2026, 10, 1, 9, 30, 0), null), template);
    }

    [Fact]
    public async Task Templates_Get()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.OK, TemplateJson);

        var template = await new TemplateApiClient(handler.CreateClient(), Store()).GetAsync(2);

        Assert.Equal("/api/templates/2", handler.LastRequest.PathAndQuery);
        Assert.Equal("Gripe", template.Title);
    }

    [Fact]
    public async Task Templates_CreateAndUpdate_SendTitleAndContent()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.OK, TemplateJson);
        var client = new TemplateApiClient(handler.CreateClient(), Store());

        await client.CreateAsync(new TemplateRequest("Gripe", "Repouso."));
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("/api/templates", handler.LastRequest.PathAndQuery);
        Assert.Equal("""{"title":"Gripe","content":"Repouso."}""", handler.LastRequest.Body);

        await client.UpdateAsync(2, new TemplateRequest("Gripe", "Repouso e água."));
        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.Equal("/api/templates/2", handler.LastRequest.PathAndQuery);
    }

    [Fact]
    public async Task Templates_DuplicatedTitle_ShowsApiReason()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.BadRequest,
                                                  """{ "detail": "Já existe um texto padrão com o título 'Gripe'." }""");

        var exception = await Assert.ThrowsAsync<ApiException>(
            () => new TemplateApiClient(handler.CreateClient(), Store()).CreateAsync(new TemplateRequest("Gripe", "x")));

        Assert.Equal("Já existe um texto padrão com o título 'Gripe'.", exception.Message);
    }

    [Fact]
    public async Task Templates_Delete()
    {
        var handler = FakeHttpMessageHandler.NoContent();

        await new TemplateApiClient(handler.CreateClient(), Store()).DeleteAsync(2);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal("/api/templates/2", handler.LastRequest.PathAndQuery);
    }

    [Fact]
    public async Task Favorites_List()
    {
        var handler = FakeHttpMessageHandler.Json(HttpStatusCode.OK, MedicationsJson);

        var favorite = Assert.Single(await new FavoriteApiClient(handler.CreateClient(), Store()).ListAsync());

        Assert.Equal("/api/favorites", handler.LastRequest.PathAndQuery);
        Assert.Equal("2", handler.LastRequest.ProfessionalId);
        Assert.Equal("Amoxicilina 500 mg", favorite.DisplayName);
    }

    [Fact]
    public async Task Favorites_AddAndRemove()
    {
        var handler = FakeHttpMessageHandler.NoContent();
        var client = new FavoriteApiClient(handler.CreateClient(), Store());

        await client.AddAsync(5);
        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.Equal("/api/favorites/5", handler.LastRequest.PathAndQuery);

        await client.RemoveAsync(5);
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal("/api/favorites/5", handler.LastRequest.PathAndQuery);
    }

    [Theory]
    [InlineData(PrescriptionFormat.Unstructured, "Não estruturada")]
    [InlineData(PrescriptionFormat.Structured, "Estruturada")]
    public void Labels_Format(PrescriptionFormat format, string label) => Assert.Equal(label, format.ToLabel());

    [Theory]
    [InlineData(PrescriptionCategory.Common, "Comum")]
    [InlineData(PrescriptionCategory.SpecialControl, "Controle especial")]
    [InlineData(PrescriptionCategory.Antimicrobial, "Antimicrobiano")]
    public void Labels_Category(PrescriptionCategory category, string label) => Assert.Equal(label, category.ToLabel());

    [Theory]
    [InlineData(Sex.Male, "Masculino")]
    [InlineData(Sex.Female, "Feminino")]
    public void Labels_Sex(Sex sex, string label) => Assert.Equal(label, sex.ToLabel());

    [Fact]
    public void Address_WithComplement()
    {
        var address = new Address("Rua B", "20", "Sala 3", "Centro", "São Paulo", "SP", "01002000");

        Assert.Equal("Rua B, 20 – Sala 3, Centro, São Paulo/SP, CEP 01002-000", address.ToString());
    }
}
