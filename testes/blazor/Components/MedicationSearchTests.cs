using System.Net;
using Prescription.Blazor.Components.Shared;
using Prescription.Blazor.Models;
using Prescription.Blazor.Tests.Fakes;

namespace Prescription.Blazor.Tests.Components;

public class MedicationSearchTests : BlazorTestContext
{
    private readonly FakeApi _api = new();

    public MedicationSearchTests()
    {
        UseStore(new FakeLoggedProfessionalStore(TestData.Ana).Loaded());
        UseAllApis(_api);
    }

    [Fact]
    public void Search_CallsApiWithTypedText()
    {
        _api.Get("/api/medications", $"[{TestJson.Amoxicillin},{TestJson.Clonazepam}]");
        var search = Render<MedicationSearch>();

        var found = SearchMedication(search, "amox");

        Assert.Equal("/api/medications?search=amox", _api.Requests.Last().PathAndQuery);
        Assert.Equal(["Amoxicilina 500 mg", "Clonazepam 2 mg"], found.Select(m => m.DisplayName));
        Assert.True(found[0].IsAntimicrobial);
        Assert.True(found[1].IsControlled);
    }

    [Fact]
    public void ApiError_ShowsMessage()
    {
        _api.Get("/api/medications", """{ "detail": "Falha na busca." }""", HttpStatusCode.BadRequest);
        var search = Render<MedicationSearch>();

        Assert.Empty(SearchMedication(search, "a"));

        search.WaitForAssertion(() => Assert.Equal("Falha na busca.",
                                                   search.Find("[data-testid=search-error]").TextContent.Trim()));
    }

    [Fact]
    public void Choose_RaisesSelectedAndKeepsName()
    {
        _api.Get("/api/medications", $"[{TestJson.Amoxicillin}]");
        Medication? chosen = null;
        var search = Render<MedicationSearch>(parameters => parameters.Add(p => p.OnSelected, (Medication m) => chosen = m));
        var medication = SearchMedication(search, "amox")[0];

        ChooseMedication(search, medication);

        Assert.Equal(5u, chosen!.Id);
        search.WaitForAssertion(() => Assert.Equal("Amoxicilina 500 mg", search.Find("input").GetAttribute("value")));
    }

    [Fact]
    public void ClearAfterChoosing_EmptiesTheField()
    {
        _api.Get("/api/medications", $"[{TestJson.Amoxicillin}]");
        Medication? chosen = null;
        var search = Render<MedicationSearch>(parameters => parameters
            .Add(p => p.OnSelected, (Medication m) => chosen = m)
            .Add(p => p.ClearAfterChoosing, true)
            .Add(p => p.Label, "Adicionar medicamento aos favoritos"));
        var medication = SearchMedication(search, "amox")[0];

        ChooseMedication(search, medication);

        Assert.Equal(5u, chosen!.Id);
        search.WaitForAssertion(() => Assert.True(string.IsNullOrEmpty(search.Find("input").GetAttribute("value"))));
        Assert.Contains("Adicionar medicamento aos favoritos", search.Markup);
    }

    /// <summary>ECF.10.07: rows of the list distinct (alternate color, separator) and highlighted (mouse over / chosen).</summary>
    [Fact]
    public void ResultList_HasDistinctRowsAndHighlight()
    {
        var css = File.ReadAllText(Path.Combine(FindSolutionFolder(), "web", "blazor", "wwwroot", "app.css"));

        Assert.Contains(".medication-list .mud-list-item:nth-child(even)", css);
        Assert.Contains(".medication-list .mud-list-item:hover", css);
        Assert.Contains(".medication-list .mud-list-item.mud-selected-item", css);
        Assert.Contains("PopoverClass=\"medication-list\"",
                        File.ReadAllText(Path.Combine(FindSolutionFolder(), "web", "blazor", "Components", "Shared",
                                                      "MedicationSearch.razor")));
    }

    private static string FindSolutionFolder()
    {
        var folder = new DirectoryInfo(AppContext.BaseDirectory);
        while (!File.Exists(Path.Combine(folder.FullName, "Prescription.sln")))
        {
            folder = folder.Parent!;
        }
        return folder.FullName;
    }
}
