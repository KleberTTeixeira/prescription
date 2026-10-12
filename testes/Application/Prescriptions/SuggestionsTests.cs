using Prescription.Application.Exceptions;
using Prescription.Application.Prescriptions;
using Prescription.Domain.Enums;
using Prescription.Tests.Application.Fakes;
using static Prescription.Tests.Application.Fakes.TestContext;

namespace Prescription.Tests.Application.Prescriptions;

public class SuggestionsTests
{
    private readonly TestContext _ctx = new();

    private SuggestPrescriptionCategory Category() => new(_ctx.Db);

    private SuggestUsageDeadline Deadline() => new(_ctx.Db);

    // ---- Category ----

    [Theory]
    [InlineData(new uint[] { }, PrescriptionCategory.Common)]
    [InlineData(new uint[] { Dipyrone }, PrescriptionCategory.Common)]
    [InlineData(new uint[] { Dipyrone, Amoxicillin }, PrescriptionCategory.Antimicrobial)]
    [InlineData(new uint[] { Clonazepam }, PrescriptionCategory.SpecialControl)]
    [InlineData(new uint[] { Amoxicillin, Clonazepam }, PrescriptionCategory.SpecialControl)]
    [InlineData(new uint[] { 99 }, PrescriptionCategory.Common)]
    public async Task Category_FollowsPrecedence(uint[] medicationIds, PrescriptionCategory expected)
    {
        Assert.Equal(expected, await Category().ExecuteAsync(medicationIds));
    }

    [Fact]
    public async Task Category_WithNull_IsCommon()
    {
        Assert.Equal(PrescriptionCategory.Common, await Category().ExecuteAsync(null));
    }

    // ---- Usage deadline (ECF.10.03) ----

    [Fact]
    public async Task Deadline_IsIssueDatePlusDays()
    {
        Assert.Equal(new DateOnly(2026, 10, 15), await Deadline().ExecuteAsync(Amoxicillin, new DateOnly(2026, 10, 5)));
        Assert.Equal(new DateOnly(2026, 11, 4), await Deadline().ExecuteAsync(Clonazepam, new DateOnly(2026, 10, 5)));
    }

    [Fact]
    public async Task Deadline_WithoutValidityLimit_IsEmpty()
    {
        Assert.Null(await Deadline().ExecuteAsync(Dipyrone, new DateOnly(2026, 10, 5)));
    }

    [Fact]
    public async Task Deadline_MedicationNotFound_Throws404()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => Deadline().ExecuteAsync(99, new DateOnly(2026, 10, 5)));
        Assert.Equal("Medicamento 99 não encontrado.", ex.Message);
    }
}
