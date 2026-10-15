using System.Net;
using MudBlazor;
using Prescription.Blazor.Components.Pages;
using Prescription.Blazor.Tests.Fakes;

namespace Prescription.Blazor.Tests.Components;

public class PrescriptionsTests : BlazorTestContext
{
    private readonly List<string> _deleted = [];

    public PrescriptionsTests() => UseStore(new FakeLoggedProfessionalStore(TestData.Ana).Loaded());

    private static string Row(int id, string client, bool issued, string format = "Structured",
                              string category = "Common", int items = 1) =>
        $$"""
          { "Id": {{id}}, "ClientName": "{{client}}", "InstitutionName": "Clínica Central", "Format": "{{format}}",
            "Category": "{{category}}", "IssuedAt": "2026-10-05T16:20:00-03:00", "IsIssued": {{issued.ToString().ToLowerInvariant()}},
            "ItemCount": {{items}} }
          """;

    private static string Page(int total, params string[] rows) =>
        $$"""{ "@odata.count": {{total}}, "value": [{{string.Join(",", rows)}}] }""";

    /// <summary>Answers the OData list with <paramref name="list"/> and records DELETEs.</summary>
    private FakeHttpMessageHandler Api(Func<string> list, HttpStatusCode deleteStatus = HttpStatusCode.NoContent,
                                       string deleteBody = "")
    {
        var handler = FakeHttpMessageHandler.Route(request =>
        {
            if (request.Method == HttpMethod.Delete)
            {
                _deleted.Add(request.RequestUri!.AbsolutePath);
                return FakeHttpMessageHandler.JsonResponse(deleteStatus, deleteBody);
            }
            return FakeHttpMessageHandler.JsonResponse(HttpStatusCode.OK, list());
        });
        UsePrescriptionApi(handler);
        return handler;
    }

    private static IEnumerable<string> ODataQueries(FakeHttpMessageHandler handler) =>
        handler.Requests.Where(r => r.Uri.AbsolutePath == "/odata/Prescriptions").Select(r => r.PathAndQuery);

    [Fact]
    public void Loads_MostRecentFirst_FirstPageOf20()
    {
        var handler = Api(() => Page(1, Row(4, "Carla Dias", issued: false)));

        var page = Render<Prescriptions>();

        page.WaitForAssertion(() => Assert.Single(page.FindAll("tbody tr.mud-table-row")));
        Assert.Equal("/odata/Prescriptions?$count=true&$orderby=IssuedAt desc,Id desc&$top=20&$skip=0",
                     ODataQueries(handler).Last());
    }

    [Fact]
    public void Row_ShowsColumnsLabelsAndSituation()
    {
        Api(() => Page(2, Row(4, "Carla Dias", issued: false, category: "Antimicrobial", items: 2),
                       Row(3, "Bruno Melo", issued: true, format: "Unstructured", items: 0)));

        var page = Render<Prescriptions>();

        page.WaitForAssertion(() => Assert.Equal(2, page.FindAll("tbody tr.mud-table-row").Count));
        var cells = page.FindAll("tbody tr.mud-table-row")[0].QuerySelectorAll("td").Select(td => td.TextContent.Trim()).ToList();
        Assert.Equal(["4", "05/10/2026 16:20", "Carla Dias", "Estruturada", "Antimicrobiano", "2", "Não emitida"], cells[..7]);
        var issuedRow = page.FindAll("tbody tr.mud-table-row")[1];
        Assert.Contains("Não estruturada", issuedRow.TextContent);
        Assert.Contains("Emitida", issuedRow.TextContent);
    }

    [Fact]
    public void Actions_EditOnlyWhenNotIssued()
    {
        Api(() => Page(2, Row(4, "Carla Dias", issued: false), Row(3, "Bruno Melo", issued: true)));

        var page = Render<Prescriptions>();

        page.WaitForAssertion(() => Assert.Equal(2, page.FindAll("tbody tr.mud-table-row").Count));
        var rows = page.FindAll("tbody tr.mud-table-row");
        Assert.Equal("receitas/4/editar", rows[0].QuerySelector("[data-testid=edit]")!.GetAttribute("href"));
        Assert.Null(rows[1].QuerySelector("[data-testid=edit]"));
        var links = rows[1].QuerySelectorAll("a").Select(a => a.GetAttribute("href")).ToList();
        Assert.Contains("receitas/3", links);
        Assert.Contains("receitas/3/imprimir", links);
        Assert.Equal("receitas/nova", page.Find("[data-testid=new]").GetAttribute("href"));
    }

    [Fact]
    public void NoRows_ShowsEmptyMessage()
    {
        Api(() => Page(0));

        var page = Render<Prescriptions>();

        page.WaitForAssertion(() => Assert.Equal("Nenhuma receita encontrada.",
                                                 page.Find("[data-testid=empty]").TextContent.Trim()));
    }

    [Fact]
    public void ApiError_ShowsMessage()
    {
        UsePrescriptionApi(FakeHttpMessageHandler.Json(HttpStatusCode.BadRequest,
                                                       """{ "detail": "Profissional 99 não encontrado." }"""));

        var page = Render<Prescriptions>();

        page.WaitForAssertion(() => Assert.Equal("Profissional 99 não encontrado.",
                                                 page.Find("[data-testid=error]").TextContent.Trim()));
    }

    [Fact]
    public void Filter_WhileTyping_SearchesByClientNameFromFirstPage()
    {
        var handler = Api(() => Page(1, Row(4, "Carla Dias", issued: false)));
        var page = Render<Prescriptions>();
        page.WaitForAssertion(() => Assert.Single(page.FindAll("tbody tr.mud-table-row")));

        Type(page, "client-name", "Carla");

        page.WaitForAssertion(() => Assert.EndsWith("&$skip=0&$filter=contains(ClientName,'Carla')",
                                                    ODataQueries(handler).Last()));
    }

    [Fact]
    public void SortByColumn_FirstClickAscending_SecondDescending()
    {
        var handler = Api(() => Page(1, Row(4, "Carla Dias", issued: false)));
        var page = Render<Prescriptions>();
        page.WaitForAssertion(() => Assert.Single(page.FindAll("tbody tr.mud-table-row")));

        page.Find("[data-testid=sort-ClientName]").Click();
        page.WaitForAssertion(() => Assert.Contains("$orderby=ClientName asc,Id asc", ODataQueries(handler).Last()));

        page.Find("[data-testid=sort-ClientName]").Click();
        page.WaitForAssertion(() => Assert.Contains("$orderby=ClientName desc,Id desc", ODataQueries(handler).Last()));
    }

    [Fact]
    public void Pager_OnlyWhenMoreThan20_NextPageSkips20()
    {
        var handler = Api(() => Page(41, Row(4, "Carla Dias", issued: false)));
        var page = Render<Prescriptions>();
        page.WaitForAssertion(() => Assert.NotNull(page.Find("[data-testid=pager]")));

        page.Find("[data-testid=pager] button[aria-label='Next page']").Click();

        page.WaitForAssertion(() => Assert.Contains("&$top=20&$skip=20", ODataQueries(handler).Last()));
    }

    [Fact]
    public void Pager_HiddenWhenUpTo20()
    {
        Api(() => Page(20, Row(4, "Carla Dias", issued: false)));

        var page = Render<Prescriptions>();

        page.WaitForAssertion(() => Assert.Single(page.FindAll("tbody tr.mud-table-row")));
        Assert.Empty(page.FindAll("[data-testid=pager]"));
    }

    [Fact]
    public void Delete_Confirmed_DeletesShowsMessageAndReloads()
    {
        var handler = Api(() => Page(1, Row(4, "Carla Dias", issued: false)));
        var dialogs = Render<MudDialogProvider>();
        var page = Render<Prescriptions>();
        page.WaitForAssertion(() => Assert.Single(page.FindAll("tbody tr.mud-table-row")));
        var listCalls = ODataQueries(handler).Count();

        page.Find("[data-testid=delete]").Click();
        dialogs.WaitForAssertion(() => Assert.Contains("Excluir a receita nº 4 de Carla Dias?", dialogs.Markup));
        dialogs.FindAll("button").Single(b => b.TextContent.Trim() == "Excluir").Click();

        page.WaitForAssertion(() => Assert.Equal(["Receita nº 4 excluída."], Notices));
        Assert.Equal(["/api/prescriptions/4"], _deleted);
        Assert.True(ODataQueries(handler).Count() > listCalls);
    }

    [Fact]
    public void Delete_Cancelled_DoesNothing()
    {
        Api(() => Page(1, Row(4, "Carla Dias", issued: false)));
        var dialogs = Render<MudDialogProvider>();
        var page = Render<Prescriptions>();
        page.WaitForAssertion(() => Assert.Single(page.FindAll("tbody tr.mud-table-row")));

        page.Find("[data-testid=delete]").Click();
        dialogs.WaitForAssertion(() => Assert.Contains("Excluir receita", dialogs.Markup));
        dialogs.FindAll("button").Single(b => b.TextContent.Trim() == "Cancelar").Click();

        dialogs.WaitForAssertion(() => Assert.DoesNotContain("Excluir receita", dialogs.Markup));
        Assert.Empty(_deleted);
        Assert.Empty(Notices);
    }

    [Fact]
    public void Delete_ApiRefuses_ShowsReason()
    {
        Api(() => Page(1, Row(4, "Carla Dias", issued: false)), HttpStatusCode.BadRequest,
            """{ "detail": "Receita pertence a outro profissional." }""");
        var dialogs = Render<MudDialogProvider>();
        var page = Render<Prescriptions>();
        page.WaitForAssertion(() => Assert.Single(page.FindAll("tbody tr.mud-table-row")));

        page.Find("[data-testid=delete]").Click();
        dialogs.WaitForAssertion(() => Assert.Contains("Excluir receita", dialogs.Markup));
        dialogs.FindAll("button").Single(b => b.TextContent.Trim() == "Excluir").Click();

        page.WaitForAssertion(() => Assert.Equal("Receita pertence a outro profissional.",
                                                 page.Find("[data-testid=error]").TextContent.Trim()));
    }
}
