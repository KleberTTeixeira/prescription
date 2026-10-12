using Prescription.Application.Exceptions;
using Prescription.Application.Prescriptions;
using Prescription.Domain.Enums;
using Prescription.Domain.Exceptions;
using Prescription.Tests.Application.Fakes;
using static Prescription.Tests.Application.Fakes.TestContext;

namespace Prescription.Tests.Application.Prescriptions;

public class CreatePrescriptionTests
{
    private readonly TestContext _ctx = new();

    [Fact]
    public async Task Unstructured_IsSaved_WithIssuedAtNow_AndNames()
    {
        var dto = await _ctx.CreateAsync(Unstructured("Paracetamol 750 mg"));

        Assert.Equal(1, _ctx.Db.SaveCount);
        Assert.Single(_ctx.Db.Prescriptions);
        Assert.NotEqual(0u, dto.Id);
        Assert.Equal(Now, dto.IssuedAt);
        Assert.Null(dto.PrintedAt);
        Assert.False(dto.IsIssued);
        Assert.Equal(Ana, dto.ProfessionalId);
        Assert.Equal("Ana Beatriz Moreira", dto.ProfessionalName);
        Assert.Equal("João Pedro Silva", dto.ClientName);
        Assert.Equal("Clínica Vida Plena", dto.InstitutionName);
        Assert.Equal(PrescriptionFormat.Unstructured, dto.Format);
        Assert.Equal("Paracetamol 750 mg", dto.FreeText);
        Assert.Empty(dto.Items);
    }

    [Fact]
    public async Task CatalogItem_TakesNameAndFormFromCatalog_IgnoringRequest()
    {
        var dto = await _ctx.CreateAsync(Structured(CatalogItem(Amoxicillin, name: "Outro nome", formId: Tablet)));

        var item = Assert.Single(dto.Items);
        Assert.Equal(Amoxicillin, item.MedicationId);
        Assert.Equal("Amoxicilina 500 mg", item.MedicationName);
        Assert.Equal(Capsule, item.PharmaceuticalFormId);
        Assert.Equal("Cápsula", item.PharmaceuticalFormName);
        Assert.Equal("mg", item.DoseUnitAbbreviation);
        Assert.Equal("1 vez ao dia", item.FrequencyName);
        Assert.Equal("Oral", item.AdministrationRouteName);
        Assert.Equal(new DateOnly(2026, 10, 21), item.UsageDeadline);
        Assert.Equal("Tomar após as refeições", item.Notes);
    }

    [Fact]
    public async Task CatalogItem_WithoutConcentration_UsesNameOnly()
    {
        var dto = await _ctx.CreateAsync(Structured(CatalogItem(Saline)));

        Assert.Equal("Soro Fisiológico", Assert.Single(dto.Items).MedicationName);
    }

    [Fact]
    public async Task ItemNotInCatalog_TakesNameAndFormFromRequest()
    {
        var dto = await _ctx.CreateAsync(Structured(FreeItem("Cápsula de vitamina D3", Capsule)));

        var item = Assert.Single(dto.Items);
        Assert.Null(item.MedicationId);
        Assert.Equal("Cápsula de vitamina D3", item.MedicationName);
        Assert.Equal(Capsule, item.PharmaceuticalFormId);
        Assert.True(item.IsContinuousUse);
        Assert.Null(item.UsageDeadline);
    }

    [Fact]
    public async Task Structured_WithFreeTextAsGeneralNote_IsSaved()
    {
        var request = Structured(CatalogItem(Dipyrone)) with { FreeText = "Retornar em 7 dias" };

        var dto = await _ctx.CreateAsync(request);

        Assert.Equal("Retornar em 7 dias", dto.FreeText);
    }

    [Fact]
    public async Task ProfessionalNotFound_Throws_AndNothingIsSaved()
    {
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _ctx.CreateAsync(Unstructured(), professionalId: 99));

        Assert.Equal("Profissional 99 não encontrado.", ex.Message);
        Assert.Equal(0, _ctx.Db.SaveCount);
    }

    [Fact]
    public async Task ClientNotFound_Throws()
    {
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _ctx.CreateAsync(Unstructured(clientId: 99)));
        Assert.Equal("Cliente 99 não encontrado.", ex.Message);
    }

    [Fact]
    public async Task InstitutionNotFound_Throws()
    {
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _ctx.CreateAsync(Unstructured(institutionId: 99)));
        Assert.Equal("Instituição 99 não encontrada.", ex.Message);
    }

    [Fact]
    public async Task MedicationNotFound_Throws()
    {
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _ctx.CreateAsync(Structured(CatalogItem(99))));
        Assert.Equal("Medicamento 99 não encontrado.", ex.Message);
    }

    [Fact]
    public async Task InactiveMedication_Throws()
    {
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _ctx.CreateAsync(Structured(CatalogItem(Inactive))));
        Assert.Equal("Medicamento 'Medicamento Antigo 10 mg' está inativo.", ex.Message);
        Assert.Equal(0, _ctx.Db.SaveCount);
    }

    [Fact]
    public async Task PharmaceuticalFormNotFound_ForItemNotInCatalog_Throws()
    {
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _ctx.CreateAsync(Structured(FreeItem(formId: 99))));
        Assert.Equal("Forma farmacêutica 99 não encontrada.", ex.Message);
    }

    [Fact]
    public async Task DoseUnitNotFound_Throws()
    {
        var item = CatalogItem(Dipyrone) with { DoseUnitId = 99 };

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _ctx.CreateAsync(Structured(item)));
        Assert.Equal("Unidade de dose 99 não encontrada.", ex.Message);
    }

    [Fact]
    public async Task FrequencyNotFound_Throws()
    {
        var item = CatalogItem(Dipyrone) with { FrequencyId = 99 };

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _ctx.CreateAsync(Structured(item)));
        Assert.Equal("Frequência 99 não encontrada.", ex.Message);
    }

    [Fact]
    public async Task AdministrationRouteNotFound_Throws()
    {
        var item = CatalogItem(Dipyrone) with { AdministrationRouteId = 99 };

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _ctx.CreateAsync(Structured(item)));
        Assert.Equal("Via de administração 99 não encontrada.", ex.Message);
    }

    [Fact]
    public async Task NullItem_Throws()
    {
        var request = new PrescriptionRequest(Joao, Clinic, PrescriptionFormat.Structured, PrescriptionCategory.Common,
                                              null, [null!]);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _ctx.CreateAsync(request));
        Assert.Equal("Item da receita inválido.", ex.Message);
    }

    [Fact]
    public async Task DomainRule_Ecf1001_IsApplied_AndNothingIsSaved()
    {
        var ex = await Assert.ThrowsAsync<DomainException>(() => _ctx.CreateAsync(Unstructured("   ")));

        Assert.Equal("Receita não estruturada exige texto livre.", ex.Message);
        Assert.Equal(0, _ctx.Db.SaveCount);
        Assert.Empty(_ctx.Db.Prescriptions);
    }

    [Fact]
    public async Task DomainRule_StructuredWithoutItems_IsApplied()
    {
        var ex = await Assert.ThrowsAsync<DomainException>(() => _ctx.CreateAsync(Structured()));
        Assert.Equal("Receita estruturada exige no mínimo 1 item.", ex.Message);
    }

    [Fact]
    public async Task ItemNotInCatalog_WithoutName_IsRejectedByDomain()
    {
        var ex = await Assert.ThrowsAsync<DomainException>(() => _ctx.CreateAsync(Structured(FreeItem(name: " "))));
        Assert.Equal("Campo 'Nome do medicamento' é obrigatório.", ex.Message);
    }

    [Fact]
    public async Task ItemNotInCatalog_WithoutForm_IsRejectedByDomain()
    {
        var item = FreeItem() with { PharmaceuticalFormId = null };

        var ex = await Assert.ThrowsAsync<DomainException>(() => _ctx.CreateAsync(Structured(item)));
        Assert.Equal("Campo 'Forma farmacêutica' é obrigatório.", ex.Message);
    }
}
