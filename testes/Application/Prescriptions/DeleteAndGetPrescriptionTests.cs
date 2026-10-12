using Prescription.Application.Exceptions;
using Prescription.Application.Prescriptions;
using Prescription.Tests.Application.Fakes;
using static Prescription.Tests.Application.Fakes.TestContext;

namespace Prescription.Tests.Application.Prescriptions;

public class DeleteAndGetPrescriptionTests
{
    private readonly TestContext _ctx = new();

    private DeletePrescription Delete() => new(_ctx.Db, _ctx.Db);

    private GetPrescription Get() => new(_ctx.Db);

    [Fact]
    public async Task Delete_RemovesPrescription()
    {
        var created = await _ctx.CreateAsync(Unstructured());

        await Delete().ExecuteAsync(Ana, created.Id);

        Assert.Empty(_ctx.Db.Prescriptions);
    }

    [Fact]
    public async Task Delete_IssuedPrescription_IsAllowed()
    {
        var created = await _ctx.CreateAsync(Unstructured());
        _ctx.Db.Prescriptions.Single().MarkAsPrinted(Now);

        await Delete().ExecuteAsync(Ana, created.Id);

        Assert.Empty(_ctx.Db.Prescriptions);
    }

    [Fact]
    public async Task Delete_ByOtherProfessional_Throws()
    {
        var created = await _ctx.CreateAsync(Unstructured());

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => Delete().ExecuteAsync(Carlos, created.Id));

        Assert.Equal("Receita pertence a outro profissional.", ex.Message);
        Assert.Single(_ctx.Db.Prescriptions);
    }

    [Fact]
    public async Task Delete_NotFound_Throws404()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => Delete().ExecuteAsync(Ana, 99));
        Assert.Equal("Receita 99 não encontrada.", ex.Message);
    }

    [Fact]
    public async Task Get_ReturnsPrescription()
    {
        var created = await _ctx.CreateAsync(Structured(CatalogItem(Amoxicillin)));

        var dto = await Get().ExecuteAsync(created.Id);

        Assert.Equal(created.Id, dto.Id);
        Assert.Equal("Amoxicilina 500 mg", Assert.Single(dto.Items).MedicationName);
    }

    [Fact]
    public async Task Get_NotFound_Throws404()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => Get().ExecuteAsync(99));
        Assert.Equal("Receita 99 não encontrada.", ex.Message);
    }
}
