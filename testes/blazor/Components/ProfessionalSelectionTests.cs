using System.Net;
using Prescription.Blazor.Components.Pages;
using Prescription.Blazor.Tests.Fakes;

namespace Prescription.Blazor.Tests.Components;

public class ProfessionalSelectionTests : BlazorTestContext
{
    public ProfessionalSelectionTests() => UseStore(new FakeLoggedProfessionalStore());

    [Fact]
    public void ListsProfessionalsAsCardsWithInitialsAndCouncil()
    {
        UseApi(FakeHttpMessageHandler.Json(HttpStatusCode.OK, TestData.ProfessionalsJson));

        var page = Render<ProfessionalSelection>();

        var cards = page.FindAll("[data-testid=professional]");
        Assert.Equal(2, cards.Count);
        Assert.Contains("AS", cards[0].TextContent);
        Assert.Contains("Ana Souza", cards[0].TextContent);
        Assert.Contains("CRM 12345/SP", cards[0].TextContent);
        Assert.Contains("Bruno Lima", cards[1].TextContent);
        Assert.Contains("Escolha o profissional para continuar", page.Markup);
    }

    [Fact]
    public void ApiError_ShowsMessage()
    {
        UseApi(FakeHttpMessageHandler.Throws(new HttpRequestException("recusado")));

        var page = Render<ProfessionalSelection>();

        Assert.Equal("Não foi possível conectar ao servidor.",
                     page.Find("[data-testid=error]").TextContent.Trim());
    }

    [Fact]
    public void NoProfessionals_ShowsEmptyMessage()
    {
        UseApi(FakeHttpMessageHandler.Json(HttpStatusCode.OK, "[]"));

        var page = Render<ProfessionalSelection>();

        Assert.Equal("Nenhum profissional cadastrado.", page.Find("[data-testid=empty]").TextContent.Trim());
    }

    [Fact]
    public void Choose_StoresProfessionalAndOpensPrescriptions()
    {
        UseApi(FakeHttpMessageHandler.Json(HttpStatusCode.OK, TestData.ProfessionalsJson));
        var page = Render<ProfessionalSelection>();

        page.FindAll("[data-testid=professional]")[1].Click();

        Assert.Equal(TestData.Bruno, Store.Current);
        Assert.Equal("/receitas", CurrentPath);
    }
}
