using Prescription.Domain.Entities;
using Prescription.Domain.Exceptions;

namespace Prescription.Tests.Domain.Entities;

/// <summary>Lookup tables: pharmaceutical_forms, administration_routes, dose_units, frequencies, validity_periods.</summary>
public class LookupEntitiesTests
{
    [Fact]
    public void PharmaceuticalForm_Valid_And_Invalid()
    {
        Assert.Equal("Comprimido", new PharmaceuticalForm("Comprimido").Name);
        Assert.Equal("Campo 'Forma farmacêutica' é obrigatório.",
                     Assert.Throws<DomainException>(() => new PharmaceuticalForm(" ")).Message);
        Assert.Equal("Campo 'Forma farmacêutica' deve ter no máximo 60 caracteres.",
                     Assert.Throws<DomainException>(() => new PharmaceuticalForm(TestData.Text(61))).Message);
    }

    [Fact]
    public void AdministrationRoute_Valid_And_Invalid()
    {
        Assert.Equal("Oral", new AdministrationRoute("Oral").Name);
        Assert.Equal("Campo 'Via de administração' é obrigatório.",
                     Assert.Throws<DomainException>(() => new AdministrationRoute("")).Message);
        Assert.Equal("Campo 'Via de administração' deve ter no máximo 60 caracteres.",
                     Assert.Throws<DomainException>(() => new AdministrationRoute(TestData.Text(61))).Message);
    }

    [Fact]
    public void DoseUnit_Valid_And_Invalid()
    {
        var unit = new DoseUnit("mg", "Miligrama");
        Assert.Equal("mg", unit.Abbreviation);
        Assert.Equal("Miligrama", unit.Name);

        Assert.Equal("Campo 'Abreviação da unidade' é obrigatório.",
                     Assert.Throws<DomainException>(() => new DoseUnit("", "Miligrama")).Message);
        Assert.Equal("Campo 'Abreviação da unidade' deve ter no máximo 20 caracteres.",
                     Assert.Throws<DomainException>(() => new DoseUnit(TestData.Text(21), "Miligrama")).Message);
        Assert.Equal("Campo 'Unidade de dose' é obrigatório.",
                     Assert.Throws<DomainException>(() => new DoseUnit("mg", "")).Message);
        Assert.Equal("Campo 'Unidade de dose' deve ter no máximo 60 caracteres.",
                     Assert.Throws<DomainException>(() => new DoseUnit("mg", TestData.Text(61))).Message);
    }

    [Fact]
    public void Frequency_Valid_And_Invalid()
    {
        Assert.Equal("8 em 8 horas", new Frequency("8 em 8 horas").Name);
        Assert.Equal("Campo 'Frequência' é obrigatório.",
                     Assert.Throws<DomainException>(() => new Frequency("")).Message);
        Assert.Equal("Campo 'Frequência' deve ter no máximo 60 caracteres.",
                     Assert.Throws<DomainException>(() => new Frequency(TestData.Text(61))).Message);
    }

    [Fact]
    public void ValidityPeriod_WithDays_And_WithoutDays()
    {
        var withDays = new ValidityPeriod("Antimicrobiano - 10 dias", 10);
        Assert.Equal("Antimicrobiano - 10 dias", withDays.Name);
        Assert.Equal(10u, withDays.Days);

        Assert.Null(new ValidityPeriod("Sem prazo", null).Days);
    }

    [Fact]
    public void ValidityPeriod_Invalid()
    {
        Assert.Equal("Campo 'Dias do prazo de utilização' deve ser maior que zero.",
                     Assert.Throws<DomainException>(() => new ValidityPeriod("Prazo", 0)).Message);
        Assert.Equal("Campo 'Prazo de utilização' é obrigatório.",
                     Assert.Throws<DomainException>(() => new ValidityPeriod("", 10)).Message);
        Assert.Equal("Campo 'Prazo de utilização' deve ter no máximo 80 caracteres.",
                     Assert.Throws<DomainException>(() => new ValidityPeriod(TestData.Text(81), 10)).Message);
    }
}
