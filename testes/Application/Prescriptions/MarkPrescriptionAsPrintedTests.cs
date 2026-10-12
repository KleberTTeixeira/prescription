using Prescription.Application.Exceptions;
using Prescription.Application.Prescriptions;
using Prescription.Tests.Application.Fakes;
using static Prescription.Tests.Application.Fakes.TestContext;

namespace Prescription.Tests.Application.Prescriptions;

public class MarkPrescriptionAsPrintedTests
{
    private readonly TestContext _ctx = new();

    private MarkPrescriptionAsPrinted UseCase() => new(_ctx.Db, _ctx.Db, _ctx.Clock);

    [Fact]
    public async Task FirstConfirmedPrint_IssuesPrescription()
    {
        var created = await _ctx.CreateAsync(Unstructured());
        _ctx.Clock.Now = Now.AddMinutes(15);

        var dto = await UseCase().ExecuteAsync(Ana, created.Id);

        Assert.Equal(Now.AddMinutes(15), dto.PrintedAt);
        Assert.True(dto.IsIssued);
        Assert.Equal(2, _ctx.Db.SaveCount);
    }

    [Fact]
    public async Task SecondConfirmedPrint_KeepsFirstDate()
    {
        var created = await _ctx.CreateAsync(Unstructured());
        _ctx.Clock.Now = Now.AddMinutes(15);
        await UseCase().ExecuteAsync(Ana, created.Id);
        _ctx.Clock.Now = Now.AddDays(3);

        var dto = await UseCase().ExecuteAsync(Ana, created.Id);

        Assert.Equal(Now.AddMinutes(15), dto.PrintedAt);
    }

    [Fact]
    public async Task ByOtherProfessional_Throws()
    {
        var created = await _ctx.CreateAsync(Unstructured());

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => UseCase().ExecuteAsync(Carlos, created.Id));

        Assert.Equal("Receita pertence a outro profissional.", ex.Message);
        Assert.Null(_ctx.Db.Prescriptions.Single().PrintedAt);
    }

    [Fact]
    public async Task NotFound_Throws404()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => UseCase().ExecuteAsync(Ana, 99));
        Assert.Equal("Receita 99 não encontrada.", ex.Message);
    }
}
