using Prescription.Application.Catalog;
using Prescription.Domain.Enums;

namespace Prescription.IntegrationTests;

/// <summary>Reads of the catalog against 02_seed.sql.</summary>
public class CatalogTests : DatabaseTest
{
    [SkippableFact]
    public async Task Lookups_AreReadOrderedById()
    {
        RequireDatabase();

        var forms = await Get<ListPharmaceuticalForms>().ExecuteAsync();
        var routes = await Get<ListAdministrationRoutes>().ExecuteAsync();
        var units = await Get<ListDoseUnits>().ExecuteAsync();
        var frequencies = await Get<ListFrequencies>().ExecuteAsync();
        var periods = await Get<ListValidityPeriods>().ExecuteAsync();

        Assert.Equal(10, forms.Count);
        Assert.Equal(new LookupDto(1, "Comprimido"), forms[0]);
        Assert.Equal(10, routes.Count);
        Assert.Equal(new DoseUnitDto(6, "comp", "Comprimido"), units[5]);
        Assert.Equal(8, frequencies.Count);
        Assert.Equal("1 vez ao dia (24 em 24 horas)", frequencies[0].Name);
        Assert.Equal("Se necessário", frequencies[7].Name);
        Assert.Equal([null, 10u, 90u, 30u], periods.Select(p => p.Days));
    }

    [SkippableFact]
    public async Task Clients_AreReadOrderedByName_WithAddressDatesAndSex()
    {
        RequireDatabase();

        var clients = await Get<ListClients>().ExecuteAsync();

        Assert.Equal([6u, 4u, 1u, 3u, 2u, 5u], clients.Select(c => c.Id));
        var joao = clients.Single(c => c.Id == 1);
        Assert.Equal("João Pedro Silva", joao.FullName);
        Assert.Equal(new DateOnly(1980, 3, 15), joao.BirthDate);
        Assert.Equal(Sex.Male, joao.Sex);
        Assert.Equal(new AddressDto("Rua Augusta", "500", "Apto 12", "Consolação", "São Paulo", "SP", "01304000"),
                     joao.Address);
        Assert.Equal(Sex.Female, clients.Single(c => c.Id == 2).Sex);
        Assert.Null(clients.Single(c => c.Id == 2).Address.Complement);
    }

    [SkippableFact]
    public async Task Professionals_AndInstitutions_AreReadOrderedByName()
    {
        RequireDatabase();

        var professionals = await Get<ListProfessionals>().ExecuteAsync();
        var institutions = await Get<ListInstitutions>().ExecuteAsync();

        Assert.Equal([1u, 2u, 3u, 4u], professionals.Select(p => p.Id));
        Assert.Equal(new ProfessionalDto(4, "Ricardo Alves Pinto", "CRO", "456789", "RJ"), professionals[3]);
        Assert.Equal([3u, 1u, 2u, 4u], institutions.Select(i => i.Id));
        var santaClara = institutions.Single(i => i.Id == 2);
        Assert.Equal("2345678", santaClara.Cnes);
        Assert.Equal("Bloco B", santaClara.Address.Complement);
        Assert.Equal("(19) 3200-2002", santaClara.Phone);
    }

    [SkippableTheory]
    [InlineData("amox", new uint[] { 6, 5 })]
    [InlineData("DIPIRONA", new uint[] { 1, 2 })]
    [InlineData("sodica", new uint[] { 1, 2 })]          // accent insensitive (Sódica)
    [InlineData("sertralina", new uint[] { 16 })]        // active ingredient "Cloridrato de sertralina"
    [InlineData("cefalexína", new uint[] { 8 })]        // accent typed by the user is ignored too
    [InlineData("inexistente", new uint[] { })]
    [InlineData("%", new uint[] { })]                    // % is plain text, not a wildcard
    [InlineData("_", new uint[] { })]                    // _ is plain text, not a wildcard
    public async Task SearchMedications_ByNameOrIngredient(string text, uint[] expected)
    {
        RequireDatabase();

        var result = await Get<SearchMedications>().ExecuteAsync(text);

        Assert.Equal(expected, result.Select(m => m.Id));
    }

    [SkippableFact]
    public async Task SearchMedications_MapsFormAndValidityDays()
    {
        RequireDatabase();

        var amoxicillin = (await Get<SearchMedications>().ExecuteAsync("amox")).Single(m => m.Id == 5);

        Assert.Equal("Amoxicilina 500 mg", amoxicillin.DisplayName);
        Assert.Equal("Cápsula", amoxicillin.PharmaceuticalFormName);
        Assert.Equal(10u, amoxicillin.ValidityPeriodDays);
        Assert.True(amoxicillin.IsAntimicrobial);
        Assert.False(amoxicillin.IsControlled);
    }
}
