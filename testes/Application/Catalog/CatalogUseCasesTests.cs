using Prescription.Application.Catalog;
using Prescription.Domain.Enums;
using Prescription.Tests.Application.Fakes;
using static Prescription.Tests.Application.Fakes.TestContext;

namespace Prescription.Tests.Application.Catalog;

public class CatalogUseCasesTests
{
    private readonly TestContext _ctx = new();

    [Theory]
    [InlineData("amox", new uint[] { Amoxicillin })]
    [InlineData("  CLONA  ", new uint[] { Clonazepam })]
    [InlineData("antigo", new uint[] { })]
    public async Task SearchMedications_ByNameOrIngredient_ActiveOnly(string text, uint[] expected)
    {
        var result = await new SearchMedications(_ctx.Db).ExecuteAsync(text);

        Assert.Equal(expected, result.Select(m => m.Id));
    }

    [Fact]
    public async Task SearchMedications_EmptyText_ReturnsAllActive()
    {
        var result = await new SearchMedications(_ctx.Db).ExecuteAsync(null);

        Assert.Equal(4, result.Count);
        Assert.DoesNotContain(result, m => m.Id == Inactive);
    }

    [Fact]
    public async Task SearchMedications_MapsFields()
    {
        var clonazepam = Assert.Single(await new SearchMedications(_ctx.Db).ExecuteAsync("Clonazepam"));

        Assert.Equal("Clonazepam", clonazepam.Name);
        Assert.Equal("2 mg", clonazepam.Concentration);
        Assert.Equal("Clonazepam 2 mg", clonazepam.DisplayName);
        Assert.Equal(Tablet, clonazepam.PharmaceuticalFormId);
        Assert.Equal("Comprimido", clonazepam.PharmaceuticalFormName);
        Assert.Equal(30u, clonazepam.ValidityPeriodDays);
        Assert.True(clonazepam.IsControlled);
        Assert.False(clonazepam.IsAntimicrobial);
    }

    [Fact]
    public async Task ListClients_MapsFieldsAndAddress()
    {
        var joao = (await new ListClients(_ctx.Db).ExecuteAsync()).Single(c => c.Id == Joao);

        Assert.Equal("João Pedro Silva", joao.FullName);
        Assert.Equal(new DateOnly(1980, 3, 15), joao.BirthDate);
        Assert.Equal(Sex.Male, joao.Sex);
        Assert.Equal(new AddressDto("Rua Augusta", "500", "Apto 12", "Consolação", "São Paulo", "SP", "01304000"),
                     joao.Address);
    }

    [Fact]
    public async Task ListProfessionals_MapsFields()
    {
        var ana = (await new ListProfessionals(_ctx.Db).ExecuteAsync()).Single(p => p.Id == Ana);

        Assert.Equal(new Prescription.Application.Catalog.ProfessionalDto(Ana, "Ana Beatriz Moreira", "CRM", "123456", "SP"), ana);
    }

    [Fact]
    public async Task ListInstitutions_MapsFields()
    {
        var clinic = Assert.Single(await new ListInstitutions(_ctx.Db).ExecuteAsync());

        Assert.Equal("Clínica Vida Plena", clinic.Name);
        Assert.Equal("1234567", clinic.Cnes);
        Assert.Equal("Jardim Paulista", clinic.Address.District);
        Assert.Equal("(11) 3000-1001", clinic.Phone);
    }

    [Fact]
    public async Task Lookups_MapFields()
    {
        Assert.Equal([new LookupDto(Tablet, "Comprimido"), new LookupDto(Capsule, "Cápsula")],
                     await new ListPharmaceuticalForms(_ctx.Db).ExecuteAsync());
        Assert.Equal([new LookupDto(Oral, "Oral")], await new ListAdministrationRoutes(_ctx.Db).ExecuteAsync());
        Assert.Equal([new DoseUnitDto(Mg, "mg", "Miligrama")], await new ListDoseUnits(_ctx.Db).ExecuteAsync());
        Assert.Equal([new LookupDto(OncePerDay, "1 vez ao dia")], await new ListFrequencies(_ctx.Db).ExecuteAsync());

        var periods = await new ListValidityPeriods(_ctx.Db).ExecuteAsync();
        Assert.Contains(new ValidityPeriodDto(1, "Sem prazo", null), periods);
        Assert.Contains(new ValidityPeriodDto(2, "Antimicrobiano - 10 dias", 10), periods);
    }
}
