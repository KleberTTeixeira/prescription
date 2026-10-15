using System.Net;
using Prescription.Blazor.Components.Pages;
using Prescription.Blazor.Tests.Fakes;

namespace Prescription.Blazor.Tests.Components;

public class FavoritesTests : BlazorTestContext
{
    private readonly FakeApi _api = new();

    public FavoritesTests()
    {
        UseStore(new FakeLoggedProfessionalStore(TestData.Ana).Loaded());
        UseAllApis(_api);
        _api.Get("/api/favorites", $"[{TestJson.Clonazepam}]")
            .Get("/api/medications", $"[{TestJson.Amoxicillin}]")
            .On(HttpMethod.Put, "/api/favorites/5", "", HttpStatusCode.NoContent)
            .On(HttpMethod.Delete, "/api/favorites/16", "", HttpStatusCode.NoContent);
    }

    private IRenderedComponent<Favorites> RenderPage()
    {
        var page = Render<Favorites>();
        page.WaitForAssertion(() => Assert.Empty(page.FindAll("[data-testid=loading]")));
        return page;
    }

    [Fact]
    public void ListsFavoritesAsCards()
    {
        var page = RenderPage();

        var favorite = Assert.Single(page.FindAll("[data-testid=favorite]"));
        Assert.Contains("Clonazepam 2 mg", favorite.TextContent);
        Assert.Contains("Comprimido · clonazepam", favorite.TextContent);
        Assert.Contains("Controlado", favorite.TextContent);
        Assert.Equal("1", _api.To(HttpMethod.Get, "/api/favorites").Single().ProfessionalId);
    }

    [Fact]
    public void NoFavorites_ShowsEmptyMessage()
    {
        _api.Get("/api/favorites", "[]");

        var page = RenderPage();

        Assert.Equal("Nenhum medicamento favorito.", page.Find("[data-testid=empty]").TextContent.Trim());
    }

    [Fact]
    public void ChooseInAutocomplete_AddsShowsNoticeAndReloads()
    {
        var page = RenderPage();

        ChooseMedication(page, SearchMedication(page, "amox")[0]);

        page.WaitForAssertion(() => Assert.Equal(["Amoxicilina 500 mg adicionado aos favoritos."], Notices));
        Assert.Single(_api.To(HttpMethod.Put, "/api/favorites/5"));
        Assert.Equal(2, _api.To(HttpMethod.Get, "/api/favorites").Count);
    }

    [Fact]
    public void Remove_Confirmed_DeletesShowsNoticeAndReloads()
    {
        var dialogs = Render<MudBlazor.MudDialogProvider>();
        var page = RenderPage();

        page.Find("[data-testid=remove]").Click();
        dialogs.WaitForAssertion(() => Assert.Contains("Remover o medicamento \"Clonazepam 2 mg\" dos favoritos?", dialogs.Markup));
        Assert.Empty(_api.To(HttpMethod.Delete, "/api/favorites/16"));
        dialogs.FindAll("button").Single(b => b.TextContent.Trim() == "Remover").Click();

        page.WaitForAssertion(() => Assert.Equal(["Clonazepam 2 mg removido dos favoritos."], Notices));
        Assert.Single(_api.To(HttpMethod.Delete, "/api/favorites/16"));
        Assert.Equal(2, _api.To(HttpMethod.Get, "/api/favorites").Count);
    }

    [Fact]
    public void Remove_Cancelled_DoesNothing()
    {
        var dialogs = Render<MudBlazor.MudDialogProvider>();
        var page = RenderPage();

        page.Find("[data-testid=remove]").Click();
        dialogs.WaitForAssertion(() => Assert.Contains("Remover favorito", dialogs.Markup));
        dialogs.FindAll("button").Single(b => b.TextContent.Trim() == "Cancelar").Click();

        dialogs.WaitForAssertion(() => Assert.DoesNotContain("Remover favorito", dialogs.Markup));
        Assert.Empty(_api.To(HttpMethod.Delete, "/api/favorites/16"));
        Assert.Empty(Notices);
        Assert.Single(page.FindAll("[data-testid=favorite]"));
    }

    [Fact]
    public void Add_InactiveMedication_ShowsApiReason()
    {
        _api.On(HttpMethod.Put, "/api/favorites/5", """{ "detail": "Medicamento 5 inativo." }""", HttpStatusCode.BadRequest);
        var page = RenderPage();

        ChooseMedication(page, SearchMedication(page, "amox")[0]);

        page.WaitForAssertion(() => Assert.Equal("Medicamento 5 inativo.",
                                                 page.Find("[data-testid=error]").TextContent.Trim()));
        Assert.Empty(Notices);
    }

    [Fact]
    public void LoadError_ShowsMessage()
    {
        _api.Get("/api/favorites", """{ "detail": "Profissional 1 não encontrado." }""", HttpStatusCode.BadRequest);

        var page = RenderPage();

        Assert.Equal("Profissional 1 não encontrado.", page.Find("[data-testid=error]").TextContent.Trim());
    }
}
