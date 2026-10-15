using System.Net;
using MudBlazor;
using Prescription.Blazor.Components.Pages;
using Prescription.Blazor.Tests.Fakes;

namespace Prescription.Blazor.Tests.Components;

public class PrescriptionViewTests : BlazorTestContext
{
    private readonly FakeApi _api = new();

    public PrescriptionViewTests()
    {
        UseStore(new FakeLoggedProfessionalStore(TestData.Ana).Loaded());
        UseAllApis(_api);
        _api.On(HttpMethod.Delete, "/api/prescriptions/3", "", HttpStatusCode.NoContent);
    }

    private IRenderedComponent<PrescriptionView> RenderView(string json)
    {
        _api.Get("/api/prescriptions/3", json);
        var page = Render<PrescriptionView>(parameters => parameters.Add(p => p.Id, 3));
        page.WaitForAssertion(() => Assert.Empty(page.FindAll("[data-testid=loading]")));
        return page;
    }

    [Fact]
    public void ShowsDetailsAndItems()
    {
        var page = RenderView(TestJson.Prescription(freeText: "Retorno em 7 dias"));

        Assert.Equal("Receita nº 3", page.Find("[data-testid=title]").TextContent.Trim());
        var details = page.Find("[data-testid=details]").TextContent;
        Assert.Contains("05/10/2026 16:20", details);
        Assert.Contains("Maria Silva", details);
        Assert.Contains("Clínica Central", details);
        Assert.Contains("Estruturada", details);
        Assert.Contains("Antimicrobiano", details);
        Assert.Equal("Retorno em 7 dias", page.Find("[data-testid=free-text]").TextContent.Trim());
        var cells = page.Find("[data-testid=item]").QuerySelectorAll("td").Select(td => td.TextContent.Trim()).ToList();
        Assert.StartsWith("Amoxicilina 500 mg", cells[0]);
        Assert.Equal(["1,5 cp", "8/8 horas", "Oral", "05/10/2026", "7 dias", "15/10/2026", "Após as refeições"], cells[1..]);
    }

    [Fact]
    public void Owner_NotIssued_CanPrintEditDelete()
    {
        var page = RenderView(TestJson.Prescription());

        Assert.Equal("Imprimir", page.Find("[data-testid=print]").TextContent.Trim());
        Assert.Equal("receitas/3/editar", page.Find("[data-testid=edit]").GetAttribute("href"));
        Assert.NotNull(page.Find("[data-testid=delete]"));
        Assert.Empty(page.FindAll("[data-testid=issued]"));
    }

    [Fact]
    public void Issued_ReprintAndDeleteOnly()
    {
        var page = RenderView(TestJson.Prescription(issued: true));

        Assert.Equal("Imprimir novamente", page.Find("[data-testid=print]").TextContent.Trim());
        Assert.Empty(page.FindAll("[data-testid=edit]"));
        Assert.NotNull(page.Find("[data-testid=delete]"));
        Assert.Contains("impressa em 05/10/2026 16:35", page.Find("[data-testid=issued]").TextContent);
    }

    [Fact]
    public void OtherProfessional_ViewAndPrintOnly()
    {
        var page = RenderView(TestJson.Prescription(professionalId: 2));

        Assert.NotNull(page.Find("[data-testid=print]"));
        Assert.Empty(page.FindAll("[data-testid=edit]"));
        Assert.Empty(page.FindAll("[data-testid=delete]"));
    }

    [Fact]
    public void NotRegisteredItem_IsMarked()
    {
        var json = TestJson.Prescription().Replace("\"medicationId\": 5", "\"medicationId\": null");

        var page = RenderView(json);

        Assert.Contains("Cápsula · não cadastrado", page.Find("[data-testid=item]").TextContent);
    }

    [Fact]
    public void NotFound_ShowsError()
    {
        _api.Get("/api/prescriptions/3", """{ "detail": "Receita 3 não encontrada." }""", HttpStatusCode.NotFound);

        var page = Render<PrescriptionView>(parameters => parameters.Add(p => p.Id, 3));

        page.WaitForAssertion(() => Assert.Equal("Receita 3 não encontrada.",
                                                 page.Find("[data-testid=error]").TextContent.Trim()));
    }

    [Fact]
    public void Delete_Confirmed_DeletesAndOpensList()
    {
        var dialogs = Render<MudDialogProvider>();
        var page = RenderView(TestJson.Prescription());

        page.Find("[data-testid=delete]").Click();
        dialogs.WaitForAssertion(() => Assert.Contains("Excluir a receita nº 3 de Maria Silva?", dialogs.Markup));
        dialogs.FindAll("button").Single(b => b.TextContent.Trim() == "Excluir").Click();

        page.WaitForAssertion(() => Assert.Equal("/receitas", CurrentPath));
        Assert.Single(_api.To(HttpMethod.Delete, "/api/prescriptions/3"));
    }

    [Fact]
    public void Delete_Cancelled_Stays()
    {
        var dialogs = Render<MudDialogProvider>();
        var page = RenderView(TestJson.Prescription());

        page.Find("[data-testid=delete]").Click();
        dialogs.WaitForAssertion(() => Assert.Contains("Excluir receita", dialogs.Markup));
        dialogs.FindAll("button").Single(b => b.TextContent.Trim() == "Cancelar").Click();

        dialogs.WaitForAssertion(() => Assert.DoesNotContain("Excluir receita", dialogs.Markup));
        Assert.Empty(_api.To(HttpMethod.Delete, "/api/prescriptions/3"));
    }

    /// <summary>Going from one prescription to another keeps the component: it must load the new one.</summary>
    [Fact]
    public void OtherId_LoadsTheOtherPrescription()
    {
        var page = RenderView(TestJson.Prescription());
        _api.Get("/api/prescriptions/4", TestJson.Prescription().Replace("\"id\": 3", "\"id\": 4")
                                                              .Replace("Maria Silva", "Bruno Melo"));

        page.Render(parameters => parameters.Add(p => p.Id, 4));

        page.WaitForAssertion(() => Assert.Equal("Receita nº 4", page.Find("[data-testid=title]").TextContent.Trim()));
        Assert.Contains("Bruno Melo", page.Find("[data-testid=details]").TextContent);
    }
}
