using System.Net;
using System.Text.Json;
using Prescription.Blazor.Components.Pages;
using Prescription.Blazor.Models;
using Prescription.Blazor.Tests.Fakes;

namespace Prescription.Blazor.Tests.Components;

public class PrescriptionFormTests : BlazorTestContext
{
    private readonly FakeApi _api = new();

    public PrescriptionFormTests()
    {
        UseStore(new FakeLoggedProfessionalStore(TestData.Ana).Loaded());
        UseAllApis(_api);
        _api.Get("/api/clients", TestJson.Clients)
            .Get("/api/institutions", TestJson.Institutions)
            .Get("/api/pharmaceutical-forms", TestJson.PharmaceuticalForms)
            .Get("/api/administration-routes", TestJson.AdministrationRoutes)
            .Get("/api/dose-units", TestJson.DoseUnits)
            .Get("/api/frequencies", TestJson.Frequencies)
            .Get("/api/templates", TestJson.Templates)
            .Get("/api/favorites", $"[{TestJson.Clonazepam}]")
            .Get("/api/medications", $"[{TestJson.Amoxicillin}]")
            .Get("/api/medications/5/usage-deadline", """{ "usageDeadline": "2026-10-25" }""")
            .Get("/api/medications/16/usage-deadline", """{ "usageDeadline": "2026-11-14" }""")
            .Get("/api/prescriptions/suggested-category", """{ "category": "Antimicrobial" }""")
            .On(HttpMethod.Post, "/api/prescriptions", TestJson.Prescription(), HttpStatusCode.Created)
            .On(HttpMethod.Put, "/api/prescriptions/3", TestJson.Prescription());
    }

    private IRenderedComponent<PrescriptionForm> RenderNew()
    {
        var page = Render<PrescriptionForm>();
        page.WaitForAssertion(() => Assert.NotNull(page.Find("[data-testid=save]")));
        return page;
    }

    private IRenderedComponent<PrescriptionForm> RenderEdit(int id = 3)
    {
        var page = Render<PrescriptionForm>(parameters => parameters.Add(p => p.Id, id));
        page.WaitForAssertion(() => Assert.Empty(page.FindAll("[data-testid=loading]")));
        return page;
    }

    private static void SearchAndChoose(IRenderedComponent<PrescriptionForm> page)
    {
        ChooseMedication(page, SearchMedication(page, "amox")[0]);
        page.WaitForAssertion(() => Assert.NotEmpty(page.FindAll("[data-testid=medication-name]")));
    }

    private static void FillItem(IRenderedComponent<PrescriptionForm> page, int index = 0)
    {
        page.FindAll("input[data-testid=dose]")[index].Change("1,5");
        Select(page, "client", (uint?)1);
        Select(page, "institution", (uint?)1);
        var selects = page.FindComponents<MudBlazor.MudSelect<uint?>>();
        page.InvokeAsync(() =>
        {
            foreach (var select in selects.Where(s => s.Instance.Label is "Unidade" or "Frequência" or "Via de administração"))
            {
                select.Instance.ValueChanged.InvokeAsync(select.Instance.Label switch
                {
                    "Unidade" => 7u,
                    "Frequência" => 3u,
                    _ => 1u
                });
            }
        });
    }

    private JsonElement LastBody(HttpMethod method, string path) =>
        JsonDocument.Parse(_api.To(method, path).Last().Body!).RootElement;

    [Fact]
    public void New_LoadsListsAndStartsWithOneItemStartingToday()
    {
        var page = RenderNew();

        Assert.Equal("Nova receita", page.Find("[data-testid=title]").TextContent.Trim());
        Assert.Single(page.FindAll("[data-testid=item]"));
        Assert.Equal("15/10/2026", page.Find("input[data-testid=start-date]").GetAttribute("value"));
        Assert.Single(_api.To(HttpMethod.Get, "/api/clients"));
        Assert.Single(_api.To(HttpMethod.Get, "/api/favorites"));
        Assert.All(_api.Requests, request => Assert.Equal("1", request.ProfessionalId));
    }

    [Fact]
    public void Save_Empty_ShowsDomainMessagesWithoutCallingApi()
    {
        var page = RenderNew();

        page.Find("[data-testid=save]").Click();

        var errors = page.Find("[data-testid=errors]").TextContent;
        Assert.Contains("Campo 'Cliente' é obrigatório.", errors);
        Assert.Contains("Item 1: Campo 'Medicamento' é obrigatório.", errors);
        Assert.Contains("Item 1: Campo 'Dose' deve ser maior que zero.", errors);
        var belowFields = page.FindAll(".mud-input-error").Select(e => e.TextContent.Trim()).ToList();
        Assert.Contains("Campo 'Cliente' é obrigatório.", belowFields);
        Assert.Contains("Campo 'Dose' deve ser maior que zero.", belowFields);
        Assert.Empty(_api.To(HttpMethod.Post, "/api/prescriptions"));
    }

    [Fact]
    public void ChooseMedication_ShowsItSuggestsDeadlineFromIssueDateAndCategory()
    {
        var page = RenderNew();

        SearchAndChoose(page);

        Assert.Equal("Amoxicilina 500 mg", page.Find("[data-testid=medication-name]").TextContent.Trim());
        Assert.Contains("Cápsula", page.Find("[data-testid=item]").TextContent);
        Assert.Equal("/api/medications/5/usage-deadline?issueDate=2026-10-15", _api.Requests
            .Last(r => r.Uri.AbsolutePath.EndsWith("usage-deadline")).PathAndQuery);
        page.WaitForAssertion(() => Assert.Equal("25/10/2026",
                                                 page.Find("input[data-testid=usage-deadline]").GetAttribute("value")));
        Assert.Equal("/api/prescriptions/suggested-category?medicationIds=5",
                     _api.To(HttpMethod.Get, "/api/prescriptions/suggested-category").Last().PathAndQuery);
        Assert.Contains("Sugerida pelo sistema: Antimicrobiano", page.Find("[data-testid=suggested-category]").TextContent);
    }

    [Fact]
    public void ChooseFavorite_FillsMedication()
    {
        var page = RenderNew();

        page.Find("[data-testid=favorite]").Click();

        page.WaitForAssertion(() => Assert.Equal("Clonazepam 2 mg",
                                                 page.Find("[data-testid=medication-name]").TextContent.Trim()));
        Assert.Single(_api.To(HttpMethod.Get, "/api/medications/16/usage-deadline"));
    }

    [Fact]
    public void Save_New_PostsRequestAndOpensPrescription()
    {
        var page = RenderNew();
        SearchAndChoose(page);
        FillItem(page);

        page.Find("[data-testid=save]").Click();

        page.WaitForAssertion(() => Assert.Equal("/receitas/3", CurrentPath));
        var body = LastBody(HttpMethod.Post, "/api/prescriptions");
        Assert.Equal(1u, body.GetProperty("clientId").GetUInt32());
        Assert.Equal("Structured", body.GetProperty("format").GetString());
        Assert.Equal("Antimicrobial", body.GetProperty("category").GetString());
        var item = body.GetProperty("items")[0];
        Assert.Equal(5u, item.GetProperty("medicationId").GetUInt32());
        Assert.Equal(JsonValueKind.Null, item.GetProperty("medicationName").ValueKind);
        Assert.Equal(1.5m, item.GetProperty("dose").GetDecimal());
        Assert.Equal("2026-10-15", item.GetProperty("startDate").GetString());
        Assert.Equal("2026-10-25", item.GetProperty("usageDeadline").GetString());
    }

    [Fact]
    public void NotRegistered_TypedNameAndForm_NoDeadlineCommonCategory()
    {
        var page = RenderNew();

        page.Find("[data-testid=not-registered]").Click();
        page.Find("input[data-testid=medication-not-registered]").Change("Fórmula manipulada");
        var form = page.FindComponents<MudBlazor.MudSelect<uint?>>().First(s => s.Instance.Label == "Forma farmacêutica");
        page.InvokeAsync(() => form.Instance.ValueChanged.InvokeAsync(4u));
        FillItem(page);
        page.Find("[data-testid=save]").Click();

        page.WaitForAssertion(() => Assert.Equal("/receitas/3", CurrentPath));
        var body = LastBody(HttpMethod.Post, "/api/prescriptions");
        Assert.Equal("Common", body.GetProperty("category").GetString());
        var item = body.GetProperty("items")[0];
        Assert.Equal(JsonValueKind.Null, item.GetProperty("medicationId").ValueKind);
        Assert.Equal("Fórmula manipulada", item.GetProperty("medicationName").GetString());
        Assert.Equal(4u, item.GetProperty("pharmaceuticalFormId").GetUInt32());
        Assert.Equal(JsonValueKind.Null, item.GetProperty("usageDeadline").ValueKind);
        Assert.Empty(_api.To(HttpMethod.Get, "/api/prescriptions/suggested-category"));
    }

    [Fact]
    public void Unstructured_TemplateCopiedToFreeText_SavedWithoutItems()
    {
        var page = RenderNew();

        Choose(page, "format", PrescriptionFormat.Unstructured);
        page.WaitForAssertion(() => Assert.Empty(page.FindAll("[data-testid=item]")));
        Select(page, "template", (uint?)2);
        page.Find("[data-testid=use-template]").Click();
        page.WaitForAssertion(() => Assert.Equal("Repouso e hidratação.",
                                                 page.Find("textarea[data-testid=free-text]").GetAttribute("value")));
        Select(page, "client", (uint?)1);
        Select(page, "institution", (uint?)1);
        page.Find("[data-testid=save]").Click();

        page.WaitForAssertion(() => Assert.Equal("/receitas/3", CurrentPath));
        var body = LastBody(HttpMethod.Post, "/api/prescriptions");
        Assert.Equal("Unstructured", body.GetProperty("format").GetString());
        Assert.Equal("Repouso e hidratação.", body.GetProperty("freeText").GetString());
        Assert.Equal(0, body.GetProperty("items").GetArrayLength());
    }

    [Fact]
    public void Unstructured_WithoutText_ShowsRule()
    {
        var page = RenderNew();
        Choose(page, "format", PrescriptionFormat.Unstructured);
        Select(page, "client", (uint?)1);
        Select(page, "institution", (uint?)1);

        page.Find("[data-testid=save]").Click();

        Assert.Equal("Receita não estruturada exige texto livre.", page.Find("[data-testid=errors]").TextContent.Trim());
    }

    [Fact]
    public void AddAndRemoveItems()
    {
        var page = RenderNew();

        page.Find("[data-testid=add-item]").Click();
        Assert.Equal(2, page.FindAll("[data-testid=item]").Count);

        page.FindAll("[data-testid=remove-item]")[0].Click();
        page.WaitForAssertion(() => Assert.Single(page.FindAll("[data-testid=item]")));
    }

    [Fact]
    public void Edit_LoadsPrescription_SavesWithPutAndIssueDateAsBase()
    {
        _api.Get("/api/prescriptions/3", TestJson.Prescription());
        var page = RenderEdit();

        Assert.Equal("Editar receita nº 3", page.Find("[data-testid=title]").TextContent.Trim());
        Assert.Equal("Amoxicilina 500 mg", page.Find("[data-testid=medication-name]").TextContent.Trim());
        Assert.Equal("15/10/2026", page.Find("input[data-testid=usage-deadline]").GetAttribute("value"));
        page.Find("[data-testid=add-item]").Click();
        Assert.Equal("05/10/2026", page.FindAll("input[data-testid=start-date]")[1].GetAttribute("value"));
        page.FindAll("[data-testid=remove-item]")[1].Click();

        page.Find("[data-testid=save]").Click();

        page.WaitForAssertion(() => Assert.Equal("/receitas/3", CurrentPath));
        var body = LastBody(HttpMethod.Put, "/api/prescriptions/3");
        Assert.Equal("Antimicrobial", body.GetProperty("category").GetString());
        Assert.Equal("Após as refeições", body.GetProperty("items")[0].GetProperty("notes").GetString());
    }

    [Fact]
    public void Edit_Issued_IsBlocked()
    {
        _api.Get("/api/prescriptions/3", TestJson.Prescription(issued: true));

        var page = RenderEdit();

        Assert.Equal("Receita já emitida (impressa) não pode ser alterada.",
                     page.Find("[data-testid=blocked]").TextContent.Trim());
        Assert.Empty(page.FindAll("[data-testid=save]"));
    }

    [Fact]
    public void Edit_OtherProfessional_IsBlocked()
    {
        _api.Get("/api/prescriptions/3", TestJson.Prescription(professionalId: 2));

        var page = RenderEdit();

        Assert.Equal("Receita pertence a outro profissional.", page.Find("[data-testid=blocked]").TextContent.Trim());
    }

    [Fact]
    public void Edit_NotFound_ShowsLoadError()
    {
        _api.Get("/api/prescriptions/99", """{ "detail": "Receita 99 não encontrada." }""", HttpStatusCode.NotFound);

        var page = RenderEdit(99);

        Assert.Equal("Receita 99 não encontrada.", page.Find("[data-testid=load-error]").TextContent.Trim());
    }

    [Fact]
    public void Save_ApiRefuses_ShowsReasonAndStays()
    {
        _api.On(HttpMethod.Post, "/api/prescriptions", """{ "detail": "Cliente 1 não encontrado." }""",
                HttpStatusCode.BadRequest);
        var page = RenderNew();
        SearchAndChoose(page);
        FillItem(page);

        page.Find("[data-testid=save]").Click();

        page.WaitForAssertion(() => Assert.Equal("Cliente 1 não encontrado.",
                                                 page.Find("[data-testid=errors]").TextContent.Trim()));
        Assert.NotEqual("/receitas/3", CurrentPath);
    }

    /// <summary>Found in the browser test: "Nova receita" after editing showed the data of the edited prescription.</summary>
    [Fact]
    public void Edit_ThenNew_ClearsTheScreen()
    {
        _api.Get("/api/prescriptions/3", TestJson.Prescription());
        var page = RenderEdit();
        Assert.NotEmpty(page.FindAll("[data-testid=medication-name]"));

        page.Render(parameters => parameters.Add(p => p.Id, (int?)null));

        page.WaitForAssertion(() => Assert.Equal("Nova receita", page.Find("[data-testid=title]").TextContent.Trim()));
        Assert.Empty(page.FindAll("[data-testid=medication-name]"));
        Assert.NotNull(page.Find("[data-testid=not-registered]"));
        Assert.Single(page.FindAll("[data-testid=item]"));
        Assert.Equal("15/10/2026", page.Find("input[data-testid=start-date]").GetAttribute("value"));
        Assert.True(string.IsNullOrEmpty(page.Find("input[data-testid=dose]").GetAttribute("value")));
    }

    [Fact]
    public void Edit_OtherPrescription_LoadsItsData()
    {
        _api.Get("/api/prescriptions/3", TestJson.Prescription());
        _api.Get("/api/prescriptions/4", TestJson.Prescription().Replace("\"id\": 3", "\"id\": 4")
                                                              .Replace("Após as refeições", "Em jejum"));
        var page = RenderEdit();

        page.Render(parameters => parameters.Add(p => p.Id, (int?)4));

        page.WaitForAssertion(() => Assert.Equal("Editar receita nº 4", page.Find("[data-testid=title]").TextContent.Trim()));
        Assert.Single(_api.To(HttpMethod.Get, "/api/prescriptions/4"));
        Assert.Single(page.FindAll("[data-testid=item]"));
    }

    /// <summary>Found in the browser test: on edit the closed item panel did not open on click.</summary>
    [Fact]
    public void Edit_ClosedItem_OpensOnHeaderClick()
    {
        _api.Get("/api/prescriptions/3", TestJson.Prescription());
        var page = RenderEdit();
        var panel = page.Find(".mud-expand-panel");
        Assert.DoesNotContain("mud-panel-expanded", panel.ClassList);

        page.Find(".mud-expand-panel-header").Click();

        page.WaitForAssertion(() => Assert.Contains("mud-panel-expanded", page.Find(".mud-expand-panel").ClassList));
    }

    /// <summary>The API keeps 3 decimals (1.500): the field shows 1,5, not 1,500.</summary>
    [Fact]
    public void Edit_DoseShownWithoutTrailingZeros()
    {
        _api.Get("/api/prescriptions/3", TestJson.Prescription());

        var page = RenderEdit();

        Assert.Equal("1,5", page.Find("input[data-testid=dose]").GetAttribute("value"));
        Assert.Contains("1,5 cp", page.Find("[data-testid=item]").TextContent);
    }
}
