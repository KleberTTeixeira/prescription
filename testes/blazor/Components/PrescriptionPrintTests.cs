using System.Net;
using MudBlazor;
using Prescription.Blazor.Components.Pages;
using Prescription.Blazor.Tests.Fakes;

namespace Prescription.Blazor.Tests.Components;

public class PrescriptionPrintTests : BlazorTestContext
{
    private readonly FakeApi _api = new();

    public PrescriptionPrintTests()
    {
        UseStore(new FakeLoggedProfessionalStore(TestData.Ana).Loaded());
        UseAllApis(_api);
        _api.On(HttpMethod.Post, "/api/prescriptions/3/print-confirmation", TestJson.Prescription(issued: true));
    }

    private static string PrintData(string category = "Common", string format = "Structured", string? freeText = null,
                                    bool printed = false, uint professionalId = 1, string client = """
                                        { "fullName": "Maria Silva", "address": null, "age": null, "sex": null }
                                        """) =>
        $$"""
          { "id": 3, "format": "{{format}}", "category": "{{category}}",
            "freeText": {{(freeText is null ? "null" : $"\"{freeText}\"")}},
            "issuedAt": "2026-10-05T16:20:00", "printedAt": {{(printed ? "\"2026-10-05T16:35:00\"" : "null")}},
            "client": {{client}},
            "professional": { "id": {{professionalId}}, "fullName": "Ana Souza", "council": "CRM", "councilNumber": "12345",
                              "councilState": "SP" },
            "institution": { "id": 1, "name": "Clínica Central", "cnes": "1234567", "phone": "1133334444",
                             "address": { "street": "Rua B", "number": "20", "complement": "Sala 3", "district": "Centro",
                                          "city": "São Paulo", "state": "SP", "zipCode": "01002000" } },
            "items": [{ "id": 7, "medicationId": 5, "medicationName": "Amoxicilina 500 mg", "pharmaceuticalFormId": 2,
                        "pharmaceuticalFormName": "Cápsula", "dose": 1.500, "doseUnitId": 7, "doseUnitAbbreviation": "cp",
                        "frequencyId": 3, "frequencyName": "8/8 horas", "startDate": "2026-10-05", "durationDays": 7,
                        "isContinuousUse": true, "administrationRouteId": 1, "administrationRouteName": "Oral",
                        "usageDeadline": "2026-10-15", "notes": "Após as refeições" }] }
          """;

    private IRenderedComponent<PrescriptionPrint> RenderPrint(string json)
    {
        _api.Get("/api/prescriptions/3/print-data", json);
        var page = Render<PrescriptionPrint>(parameters => parameters.Add(p => p.Id, 3));
        page.WaitForAssertion(() => Assert.NotNull(page.Find("[data-testid=sheet]")));
        return page;
    }

    [Fact]
    public void Sheet_HasInstitutionProfessionalDateAndItems()
    {
        var page = RenderPrint(PrintData());

        var institution = page.Find("[data-testid=institution]").TextContent;
        Assert.Contains("Clínica Central", institution);
        Assert.Contains("CNES 1234567", institution);
        Assert.Contains("Rua B, 20 – Sala 3, Centro, São Paulo/SP, CEP 01002-000", institution);
        Assert.Contains("Telefone: 1133334444", institution);
        Assert.Equal("Receita", page.Find("h1.sheet-title").TextContent.Trim());
        Assert.Contains("Cliente: Maria Silva", page.Find("[data-testid=client]").TextContent);
        Assert.Equal("Profissional: Ana Souza – CRM 12345/SP",
                     Normalize(page.Find("[data-testid=professional]").TextContent));
        Assert.Equal("São Paulo, 05/10/2026 16:20", page.Find("[data-testid=issued-at]").TextContent.Trim());
        var item = Normalize(page.Find("[data-testid=items] li").TextContent);
        Assert.Contains("Amoxicilina 500 mg – Cápsula", item);
        Assert.Contains("1,5 cp, 8/8 horas, via Oral. Início: 05/10/2026. Duração: 7 dias. Uso contínuo. Usar até 15/10/2026.", item);
        Assert.Contains("Após as refeições", item);
    }

    /// <summary>ECF.10.04: space for signature and stamp; the council stays in the professional block, not here.</summary>
    [Fact]
    public void Signature_BlankForSignatureAndStamp_WithoutNameOrCouncil()
    {
        var page = RenderPrint(PrintData());

        var signature = page.Find("[data-testid=signature]");
        Assert.Equal("Assinatura e carimbo do profissional", signature.TextContent.Trim());
        Assert.DoesNotContain("CRM", signature.TextContent);
        Assert.DoesNotContain("Ana Souza", signature.TextContent);
        Assert.NotNull(signature.QuerySelector(".stamp-space"));
        Assert.NotNull(signature.QuerySelector(".signature-line"));
    }

    [Fact]
    public void SpecialControl_ShowsClientAddress()
    {
        var page = RenderPrint(PrintData(category: "SpecialControl", client: """
            { "fullName": "Maria Silva", "age": null, "sex": null,
              "address": { "street": "Rua A", "number": "1", "complement": null, "district": "Centro", "city": "São Paulo",
                           "state": "SP", "zipCode": "01000000" } }
            """));

        Assert.Equal("Receituário de controle especial", page.Find("h1.sheet-title").TextContent.Trim());
        Assert.Equal("Endereço: Rua A, 1, Centro, São Paulo/SP, CEP 01000-000",
                     Normalize(page.Find("[data-testid=client-address]").TextContent));
        Assert.Empty(page.FindAll("[data-testid=client-age]"));
    }

    [Fact]
    public void Antimicrobial_ShowsAgeAndSex()
    {
        var page = RenderPrint(PrintData(category: "Antimicrobial",
                                         client: """{ "fullName": "Maria Silva", "address": null, "age": 46, "sex": "Female" }"""));

        Assert.Equal("Receita de antimicrobiano", page.Find("h1.sheet-title").TextContent.Trim());
        Assert.Equal("Idade: 46 anos", Normalize(page.Find("[data-testid=client-age]").TextContent));
        Assert.Equal("Sexo: Feminino", Normalize(page.Find("[data-testid=client-sex]").TextContent));
        Assert.Empty(page.FindAll("[data-testid=client-address]"));
    }

    [Fact]
    public void Unstructured_PrintsFreeText()
    {
        var page = RenderPrint(PrintData(format: "Unstructured", freeText: "Dipirona 500 mg se dor."));

        Assert.Equal("Dipirona 500 mg se dor.", page.Find("[data-testid=free-text]").TextContent.Trim());
        Assert.Empty(page.FindAll("[data-testid=items]"));
    }

    [Fact]
    public void Print_Confirmed_IssuesPrescriptionAndReloads()
    {
        var dialogs = Render<MudDialogProvider>();
        var page = RenderPrint(PrintData());

        page.Find("[data-testid=print]").Click();
        Assert.Single(JSInterop.Invocations, invocation => invocation.Identifier == "print");
        dialogs.WaitForAssertion(() => Assert.Contains("A receita foi impressa?", dialogs.Markup));
        _api.Get("/api/prescriptions/3/print-data", PrintData(printed: true));
        dialogs.FindAll("button").Single(b => b.TextContent.Trim() == "Sim").Click();

        page.WaitForAssertion(() => Assert.Equal(["Receita emitida. A partir de agora ela não pode mais ser alterada."], Notices));
        Assert.Single(_api.To(HttpMethod.Post, "/api/prescriptions/3/print-confirmation"));
        Assert.Equal("Emitida em 05/10/2026 16:35", page.Find("[data-testid=issued]").TextContent.Trim());
    }

    [Fact]
    public void Print_NotConfirmed_DoesNotIssue()
    {
        var dialogs = Render<MudDialogProvider>();
        var page = RenderPrint(PrintData());

        page.Find("[data-testid=print]").Click();
        dialogs.WaitForAssertion(() => Assert.Contains("A receita foi impressa?", dialogs.Markup));
        dialogs.FindAll("button").Single(b => b.TextContent.Trim() == "Não").Click();

        dialogs.WaitForAssertion(() => Assert.DoesNotContain("A receita foi impressa?", dialogs.Markup));
        Assert.Empty(_api.To(HttpMethod.Post, "/api/prescriptions/3/print-confirmation"));
        Assert.Empty(Notices);
    }

    [Theory]
    [InlineData(true, 1u)]
    [InlineData(false, 2u)]
    public void Reprint_IssuedOrOtherProfessional_DoesNotAsk(bool printed, uint professionalId)
    {
        var dialogs = Render<MudDialogProvider>();
        var page = RenderPrint(PrintData(printed: printed, professionalId: professionalId));

        page.Find("[data-testid=print]").Click();

        Assert.Single(JSInterop.Invocations, invocation => invocation.Identifier == "print");
        Assert.DoesNotContain("A receita foi impressa?", dialogs.Markup);
        Assert.Empty(_api.To(HttpMethod.Post, "/api/prescriptions/3/print-confirmation"));
    }

    [Fact]
    public void Confirm_ApiRefuses_ShowsReason()
    {
        _api.On(HttpMethod.Post, "/api/prescriptions/3/print-confirmation",
                """{ "detail": "Receita pertence a outro profissional." }""", HttpStatusCode.BadRequest);
        var dialogs = Render<MudDialogProvider>();
        var page = RenderPrint(PrintData());

        page.Find("[data-testid=print]").Click();
        dialogs.WaitForAssertion(() => Assert.Contains("A receita foi impressa?", dialogs.Markup));
        dialogs.FindAll("button").Single(b => b.TextContent.Trim() == "Sim").Click();

        page.WaitForAssertion(() => Assert.Equal("Receita pertence a outro profissional.",
                                                 page.Find("[data-testid=error]").TextContent.Trim()));
    }

    [Fact]
    public void ButtonsAndMessagesAreHiddenWhenPrinting()
    {
        var page = RenderPrint(PrintData(printed: true));

        Assert.NotNull(page.Find("[data-testid=print]").Closest(".no-print"));
    }

    [Fact]
    public void NotFound_ShowsError()
    {
        _api.Get("/api/prescriptions/3/print-data", """{ "detail": "Receita 3 não encontrada." }""", HttpStatusCode.NotFound);

        var page = Render<PrescriptionPrint>(parameters => parameters.Add(p => p.Id, 3));

        page.WaitForAssertion(() => Assert.Equal("Receita 3 não encontrada.",
                                                 page.Find("[data-testid=error]").TextContent.Trim()));
    }

    private static string Normalize(string text) => string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
