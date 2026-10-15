using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Prescription.Blazor.Components.Layout;
using Prescription.Blazor.Tests.Fakes;

namespace Prescription.Blazor.Tests.Components;

public class MainLayoutTests : BlazorTestContext
{
    private static readonly RenderFragment Page = builder => builder.AddMarkupContent(0, "<p id=page>conteúdo</p>");

    private IRenderedComponent<MainLayout> RenderLayout() =>
        Render<MainLayout>(parameters => parameters.Add(p => p.Body, Page));

    [Fact]
    public void WithoutProfessional_OpensSelection()
    {
        UseStore(new FakeLoggedProfessionalStore());

        var layout = RenderLayout();

        Assert.Equal("/profissional", CurrentPath);
        Assert.Empty(layout.FindAll("#page"));
    }

    [Fact]
    public void WithProfessional_ShowsAvatarNameCouncilAndPage()
    {
        UseStore(new FakeLoggedProfessionalStore(TestData.Ana));

        var layout = RenderLayout();

        var professional = layout.Find("[data-testid=professional]").TextContent;
        Assert.Contains("AS", professional);
        Assert.Contains("Ana Souza", professional);
        Assert.Contains("CRM 12345/SP", professional);
        Assert.NotNull(layout.Find("#page"));
        Assert.NotEqual("/profissional", CurrentPath);
    }

    [Fact]
    public void SideMenu_HasPrescriptionsTemplatesAndFavorites()
    {
        UseStore(new FakeLoggedProfessionalStore(TestData.Ana));

        var layout = RenderLayout();

        var links = layout.FindAll(".mud-drawer a").Select(a => (a.GetAttribute("href"), a.TextContent.Trim())).ToList();
        Assert.Equal([("receitas", "Receitas"), ("textos-padrao", "Textos padrão"), ("favoritos", "Favoritos")], links);
    }

    [Fact]
    public void ChangeProfessional_FromTheMenu_ClearsAndOpensSelection()
    {
        var store = new FakeLoggedProfessionalStore(TestData.Ana);
        UseStore(store);
        var layout = RenderLayout();

        // Real click on the name/avatar button.
        layout.Find("button[data-testid=professional]").Click();
        layout.WaitForAssertion(() => Assert.NotNull(layout.Find("[data-testid=change-professional]")));
        layout.Find("[data-testid=change-professional]").Click();

        Assert.Null(store.Current);
        Assert.Null(store.Stored);
        Assert.Equal("/profissional", CurrentPath);
    }
}
