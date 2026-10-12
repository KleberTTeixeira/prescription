using Prescription.Application.Exceptions;
using Prescription.Application.Prescriptions;
using Prescription.Domain.Enums;
using Prescription.Domain.Exceptions;
using Prescription.Tests.Application.Fakes;
using static Prescription.Tests.Application.Fakes.TestContext;

namespace Prescription.Tests.Application.Prescriptions;

public class UpdatePrescriptionTests
{
    private readonly TestContext _ctx = new();
    private static readonly DateTime Later = Now.AddHours(2);

    private UpdatePrescription UseCase() => new(_ctx.Db, _ctx.Resolver(), _ctx.Db, _ctx.Clock);

    [Fact]
    public async Task Update_ChangesData_SetsUpdatedAt_KeepsIssuedAtAndProfessional()
    {
        var created = await _ctx.CreateAsync(Unstructured());
        _ctx.Clock.Now = Later;

        var dto = await UseCase().ExecuteAsync(Ana, created.Id,
            Unstructured("Novo texto", clientId: Maria, category: PrescriptionCategory.Antimicrobial));

        Assert.Equal(Maria, dto.ClientId);
        Assert.Equal("Novo texto", dto.FreeText);
        Assert.Equal(PrescriptionCategory.Antimicrobial, dto.Category);
        Assert.Equal(Now, dto.IssuedAt);
        Assert.Equal(Ana, dto.ProfessionalId);
        Assert.Equal(Later, _ctx.Db.Prescriptions.Single().UpdatedAt);
        Assert.Equal(2, _ctx.Db.SaveCount);
    }

    [Fact]
    public async Task Update_ChangesFormat_BeforeIssue()
    {
        var created = await _ctx.CreateAsync(Unstructured());

        var dto = await UseCase().ExecuteAsync(Ana, created.Id, Structured(CatalogItem(Dipyrone)));

        Assert.Equal(PrescriptionFormat.Structured, dto.Format);
        Assert.Single(dto.Items);
        Assert.Null(dto.FreeText);
    }

    [Fact]
    public async Task Update_NotFound_Throws404()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => UseCase().ExecuteAsync(Ana, 99, Unstructured()));
        Assert.Equal("Receita 99 não encontrada.", ex.Message);
    }

    [Fact]
    public async Task Update_ByOtherProfessional_Throws()
    {
        var created = await _ctx.CreateAsync(Unstructured());

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            UseCase().ExecuteAsync(Carlos, created.Id, Unstructured("Outro")));

        Assert.Equal("Receita pertence a outro profissional.", ex.Message);
        Assert.Equal("Dipirona 500 mg de 6/6h", _ctx.Db.Prescriptions.Single().FreeText);
    }

    [Fact]
    public async Task Update_IssuedPrescription_IsRejected()
    {
        var created = await _ctx.CreateAsync(Unstructured());
        _ctx.Db.Prescriptions.Single().MarkAsPrinted(Later);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            UseCase().ExecuteAsync(Ana, created.Id, Unstructured("Outro")));

        Assert.Equal("Receita já emitida (impressa) não pode ser alterada.", ex.Message);
    }

    [Fact]
    public async Task Update_WithMedicationThatBecameInactive_IsAlwaysRejected()
    {
        var created = await _ctx.CreateAsync(Structured(CatalogItem(Dipyrone)));
        _ctx.Db.Medications.Single(m => m.Id == Dipyrone).With("IsActive", false);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            UseCase().ExecuteAsync(Ana, created.Id, Structured(CatalogItem(Dipyrone))));

        Assert.Equal("Medicamento 'Dipirona Sódica 500 mg' está inativo.", ex.Message);
    }

    [Fact]
    public async Task Update_WithClientNotFound_Throws()
    {
        var created = await _ctx.CreateAsync(Unstructured());

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            UseCase().ExecuteAsync(Ana, created.Id, Unstructured(clientId: 99)));

        Assert.Equal("Cliente 99 não encontrado.", ex.Message);
    }
}
