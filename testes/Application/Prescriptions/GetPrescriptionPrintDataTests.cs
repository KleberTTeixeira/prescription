using Prescription.Application.Exceptions;
using Prescription.Application.Prescriptions;
using Prescription.Domain.Enums;
using Prescription.Tests.Application.Fakes;
using static Prescription.Tests.Application.Fakes.TestContext;

namespace Prescription.Tests.Application.Prescriptions;

public class GetPrescriptionPrintDataTests
{
    private readonly TestContext _ctx = new();

    private GetPrescriptionPrintData UseCase() => new(_ctx.Db, _ctx.Clock);

    [Fact]
    public async Task Common_HasProfessionalInstitutionAndDate_WithoutClientAddressAgeOrSex()
    {
        var created = await _ctx.CreateAsync(Unstructured());

        var print = await UseCase().ExecuteAsync(created.Id);

        Assert.Equal("João Pedro Silva", print.Client.FullName);
        Assert.Null(print.Client.Address);
        Assert.Null(print.Client.Age);
        Assert.Null(print.Client.Sex);
        Assert.Equal("Ana Beatriz Moreira", print.Professional.FullName);
        Assert.Equal("CRM", print.Professional.Council);
        Assert.Equal("123456", print.Professional.CouncilNumber);
        Assert.Equal("SP", print.Professional.CouncilState);
        Assert.Equal("Clínica Vida Plena", print.Institution.Name);
        Assert.Equal("1234567", print.Institution.Cnes);
        Assert.Equal("Rua das Acácias", print.Institution.Address.Street);
        Assert.Equal("(11) 3000-1001", print.Institution.Phone);
        Assert.Equal(Now, print.IssuedAt);
        Assert.Equal("Dipirona 500 mg de 6/6h", print.FreeText);
    }

    [Fact]
    public async Task SpecialControl_IncludesClientAddress_Only()
    {
        var created = await _ctx.CreateAsync(
            Structured(PrescriptionCategory.SpecialControl, Joao, CatalogItem(Clonazepam)));

        var print = await UseCase().ExecuteAsync(created.Id);

        Assert.NotNull(print.Client.Address);
        Assert.Equal("Rua Augusta", print.Client.Address!.Street);
        Assert.Equal("Apto 12", print.Client.Address.Complement);
        Assert.Equal("01304000", print.Client.Address.ZipCode);
        Assert.Null(print.Client.Age);
        Assert.Null(print.Client.Sex);
        Assert.Equal("Clonazepam 2 mg", Assert.Single(print.Items).MedicationName);
    }

    [Fact]
    public async Task Antimicrobial_IncludesAgeAndSex_Only()
    {
        var created = await _ctx.CreateAsync(
            Structured(PrescriptionCategory.Antimicrobial, Joao, CatalogItem(Amoxicillin)));

        var print = await UseCase().ExecuteAsync(created.Id);

        Assert.Equal(46, print.Client.Age);   // born 1980-03-15, printed 2026-10-11
        Assert.Equal(Sex.Male, print.Client.Sex);
        Assert.Null(print.Client.Address);
    }

    [Fact]
    public async Task Antimicrobial_AgeIsCalculatedOnPrintDate()
    {
        var created = await _ctx.CreateAsync(
            Structured(PrescriptionCategory.Antimicrobial, Maria, CatalogItem(Amoxicillin)));

        var dayBeforeBirthday = await UseCase().ExecuteAsync(created.Id); // 2026-10-11, born 1992-10-12
        _ctx.Clock.Now = new DateTime(2026, 10, 12, 8, 0, 0);
        var onBirthday = await UseCase().ExecuteAsync(created.Id);

        Assert.Equal(33, dayBeforeBirthday.Client.Age);
        Assert.Equal(34, onBirthday.Client.Age);
        Assert.Equal(Sex.Female, onBirthday.Client.Sex);
    }

    [Fact]
    public async Task NotFound_Throws404()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => UseCase().ExecuteAsync(99));
        Assert.Equal("Receita 99 não encontrada.", ex.Message);
    }
}
