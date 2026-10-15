using System.Net;
using System.Text.Json;
using MudBlazor;
using Prescription.Blazor.Components.Pages;
using Prescription.Blazor.Tests.Fakes;

namespace Prescription.Blazor.Tests.Components;

public class TemplatesTests : BlazorTestContext
{
    private const string Template =
        """{ "id": 2, "title": "Gripe", "content": "Repouso e hidratação.", "createdAt": "2026-10-01T09:30:00", "updatedAt": null }""";

    private readonly FakeApi _api = new();

    public TemplatesTests()
    {
        UseStore(new FakeLoggedProfessionalStore(TestData.Ana).Loaded());
        UseAllApis(_api);
        _api.Get("/api/templates", $"[{Template}]")
            .On(HttpMethod.Post, "/api/templates", Template.Replace("\"id\": 2", "\"id\": 9"), HttpStatusCode.Created)
            .On(HttpMethod.Put, "/api/templates/2", Template)
            .On(HttpMethod.Delete, "/api/templates/2", "", HttpStatusCode.NoContent);
    }

    private (IRenderedComponent<MudDialogProvider> Dialogs, IRenderedComponent<Templates> Page) RenderPage()
    {
        var dialogs = Render<MudDialogProvider>();
        var page = Render<Templates>();
        page.WaitForAssertion(() => Assert.Empty(page.FindAll("[data-testid=loading]")));
        return (dialogs, page);
    }

    [Fact]
    public void ListsTemplatesOfTheProfessional()
    {
        var (_, page) = RenderPage();

        var row = Assert.Single(page.FindAll("tbody tr.mud-table-row"));
        Assert.Contains("Gripe", row.TextContent);
        Assert.Contains("Repouso e hidratação.", row.TextContent);
        Assert.Equal("1", _api.To(HttpMethod.Get, "/api/templates").Single().ProfessionalId);
    }

    [Fact]
    public void NoTemplates_ShowsEmptyMessage()
    {
        _api.Get("/api/templates", "[]");

        var (_, page) = RenderPage();

        Assert.Equal("Nenhum texto padrão cadastrado.", page.Find("[data-testid=empty]").TextContent.Trim());
    }

    [Fact]
    public void Create_InDialog_PostsTrimmedTitleAndShowsNotice()
    {
        var (dialogs, page) = RenderPage();

        page.Find("[data-testid=new]").Click();
        dialogs.WaitForAssertion(() => Assert.Contains("Novo texto padrão", dialogs.Markup));
        dialogs.Find("input[data-testid=title]").Change("  Gripe ");
        dialogs.Find("textarea[data-testid=content]").Change("Repouso e hidratação.");
        dialogs.Find("[data-testid=save]").Click();

        page.WaitForAssertion(() => Assert.Equal(["Texto padrão \"Gripe\" salvo."], Notices));
        var body = JsonDocument.Parse(_api.To(HttpMethod.Post, "/api/templates").Single().Body!).RootElement;
        Assert.Equal(("Gripe", "Repouso e hidratação."),
                     (body.GetProperty("title").GetString(), body.GetProperty("content").GetString()));
        dialogs.WaitForAssertion(() => Assert.DoesNotContain("Novo texto padrão", dialogs.Markup));
        Assert.Equal(2, _api.To(HttpMethod.Get, "/api/templates").Count);
    }

    [Fact]
    public void Edit_InDialog_FilledAndPuts()
    {
        var (dialogs, page) = RenderPage();

        page.Find("[data-testid=edit]").Click();
        dialogs.WaitForAssertion(() => Assert.Contains("Editar texto padrão", dialogs.Markup));
        Assert.Equal("Gripe", dialogs.Find("input[data-testid=title]").GetAttribute("value"));
        dialogs.Find("textarea[data-testid=content]").Change("Repouso.");
        dialogs.Find("[data-testid=save]").Click();

        page.WaitForAssertion(() => Assert.Single(_api.To(HttpMethod.Put, "/api/templates/2")));
        Assert.Contains("\"content\":\"Repouso.\"", _api.To(HttpMethod.Put, "/api/templates/2").Single().Body);
    }

    [Theory]
    [InlineData("", "Texto", "Campo 'Título' é obrigatório.")]
    [InlineData("Gripe", "   ", "Campo 'Texto' é obrigatório.")]
    public void Save_RequiredFields_MessageBelowTheField(string title, string content, string message)
    {
        var (dialogs, page) = RenderPage();
        page.Find("[data-testid=new]").Click();
        dialogs.WaitForAssertion(() => Assert.Contains("Novo texto padrão", dialogs.Markup));
        dialogs.Find("input[data-testid=title]").Change(title);
        dialogs.Find("textarea[data-testid=content]").Change(content);

        dialogs.Find("[data-testid=save]").Click();

        dialogs.WaitForAssertion(() => Assert.Contains(message, dialogs.Markup));
        Assert.Empty(_api.To(HttpMethod.Post, "/api/templates"));
    }

    [Fact]
    public void Save_DuplicatedTitle_ShowsApiReasonAndKeepsDialog()
    {
        _api.On(HttpMethod.Post, "/api/templates", """{ "detail": "Já existe um texto padrão com o título 'Gripe'." }""",
                HttpStatusCode.BadRequest);
        var (dialogs, page) = RenderPage();
        page.Find("[data-testid=new]").Click();
        dialogs.WaitForAssertion(() => Assert.Contains("Novo texto padrão", dialogs.Markup));
        dialogs.Find("input[data-testid=title]").Change("Gripe");
        dialogs.Find("textarea[data-testid=content]").Change("x");

        dialogs.Find("[data-testid=save]").Click();

        dialogs.WaitForAssertion(() => Assert.Equal("Já existe um texto padrão com o título 'Gripe'.",
                                                    dialogs.Find("[data-testid=error]").TextContent.Trim()));
        Assert.Empty(Notices);
    }

    [Fact]
    public void Cancel_ClosesDialog()
    {
        var (dialogs, page) = RenderPage();
        page.Find("[data-testid=new]").Click();
        dialogs.WaitForAssertion(() => Assert.Contains("Novo texto padrão", dialogs.Markup));

        dialogs.Find("[data-testid=cancel]").Click();

        dialogs.WaitForAssertion(() => Assert.DoesNotContain("Novo texto padrão", dialogs.Markup));
        Assert.Empty(_api.To(HttpMethod.Post, "/api/templates"));
    }

    [Fact]
    public void Delete_Confirmed_DeletesAndShowsNotice()
    {
        var (dialogs, page) = RenderPage();

        page.Find("[data-testid=delete]").Click();
        dialogs.WaitForAssertion(() => Assert.Contains("Excluir o texto padrão \"Gripe\"?", dialogs.Markup));
        dialogs.FindAll("button").Single(b => b.TextContent.Trim() == "Excluir").Click();

        page.WaitForAssertion(() => Assert.Equal(["Texto padrão \"Gripe\" excluído."], Notices));
        Assert.Single(_api.To(HttpMethod.Delete, "/api/templates/2"));
    }

    [Fact]
    public void Delete_Cancelled_DoesNothing()
    {
        var (dialogs, page) = RenderPage();

        page.Find("[data-testid=delete]").Click();
        dialogs.WaitForAssertion(() => Assert.Contains("Excluir texto padrão", dialogs.Markup));
        dialogs.FindAll("button").Single(b => b.TextContent.Trim() == "Cancelar").Click();

        dialogs.WaitForAssertion(() => Assert.DoesNotContain("Excluir texto padrão", dialogs.Markup));
        Assert.Empty(_api.To(HttpMethod.Delete, "/api/templates/2"));
    }
}
