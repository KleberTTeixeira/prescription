using Prescription.Domain.Entities;
using Prescription.Domain.Exceptions;

namespace Prescription.Tests.Domain.Entities;

public class PrescriptionItemTests
{
    private static PrescriptionItem Create(uint? medicationId = 1, string medicationName = "Amoxicilina 500 mg",
                                           uint pharmaceuticalFormId = 1, decimal dose = 1m, uint doseUnitId = 1,
                                           uint frequencyId = 1, uint? durationDays = 7,
                                           uint administrationRouteId = 1, DateOnly? usageDeadline = null,
                                           string? notes = null) =>
        new(medicationId, medicationName, pharmaceuticalFormId, dose, doseUnitId, frequencyId,
            new DateOnly(2026, 10, 11), durationDays, isContinuousUse: false, administrationRouteId,
            usageDeadline, notes);

    [Fact]
    public void Create_WithAllFields_SetsThem()
    {
        var item = new PrescriptionItem(10, "Azitromicina 500 mg", 2, 1.5m, 3, 4, new DateOnly(2026, 10, 2),
                                        5, true, 6, new DateOnly(2026, 10, 12), "Tomar em jejum");

        Assert.Equal(10u, item.MedicationId);
        Assert.Equal("Azitromicina 500 mg", item.MedicationName);
        Assert.Equal(2u, item.PharmaceuticalFormId);
        Assert.Equal(1.5m, item.Dose);
        Assert.Equal(3u, item.DoseUnitId);
        Assert.Equal(4u, item.FrequencyId);
        Assert.Equal(new DateOnly(2026, 10, 2), item.StartDate);
        Assert.Equal(5u, item.DurationDays);
        Assert.True(item.IsContinuousUse);
        Assert.Equal(6u, item.AdministrationRouteId);
        Assert.Equal(new DateOnly(2026, 10, 12), item.UsageDeadline);
        Assert.Equal("Tomar em jejum", item.Notes);
    }

    [Fact]
    public void Create_WithoutMedicationId_IsAccepted_NotInCatalog()
    {
        var item = Create(medicationId: null, medicationName: "Fórmula manipulada");

        Assert.Null(item.MedicationId);
        Assert.Equal("Fórmula manipulada", item.MedicationName);
    }

    [Fact]
    public void Create_WithMedicationIdZero_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => Create(medicationId: 0));
        Assert.Equal("Campo 'Medicamento' deve ser maior que zero.", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutMedicationName_Throws(string? name)
    {
        var ex = Assert.Throws<DomainException>(() => Create(medicationName: name!));
        Assert.Equal("Campo 'Nome do medicamento' é obrigatório.", ex.Message);
    }

    [Fact]
    public void Create_WithMedicationNameAboveMax_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => Create(medicationName: TestData.Text(151)));
        Assert.Equal("Campo 'Nome do medicamento' deve ter no máximo 150 caracteres.", ex.Message);
    }

    [Theory]
    [InlineData(0u, 1u, 1u, 1u, "Forma farmacêutica")]
    [InlineData(1u, 0u, 1u, 1u, "Unidade de dose")]
    [InlineData(1u, 1u, 0u, 1u, "Frequência")]
    [InlineData(1u, 1u, 1u, 0u, "Via de administração")]
    public void Create_WithoutRequiredId_Throws(uint formId, uint unitId, uint frequencyId, uint routeId, string field)
    {
        var ex = Assert.Throws<DomainException>(() =>
            Create(pharmaceuticalFormId: formId, doseUnitId: unitId, frequencyId: frequencyId,
                   administrationRouteId: routeId));

        Assert.Equal($"Campo '{field}' é obrigatório.", ex.Message);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    public void Create_WithDoseNotPositive_Throws(string dose)
    {
        var value = decimal.Parse(dose, System.Globalization.CultureInfo.InvariantCulture);
        var ex = Assert.Throws<DomainException>(() => Create(dose: value));
        Assert.Equal("Campo 'Dose' deve ser maior que zero.", ex.Message);
    }

    [Fact]
    public void Create_WithDoseAboveDecimal10_3_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => Create(dose: 10_000_000m));
        Assert.Equal("Campo 'Dose' deve ser no máximo 9999999,999.", ex.Message);
    }

    [Fact]
    public void Create_WithMoreThanThreeDecimals_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => Create(dose: 0.0005m));
        Assert.Equal("Campo 'Dose' deve ter no máximo 3 casas decimais.", ex.Message);
    }

    [Theory]
    [InlineData("0.001")]
    [InlineData("9999999.999")]
    public void Create_WithDoseAtLimits_IsAccepted(string dose)
    {
        var value = decimal.Parse(dose, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(value, Create(dose: value).Dose);
    }

    [Fact]
    public void Create_WithDurationZero_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => Create(durationDays: 0));
        Assert.Equal("Campo 'Duração do tratamento (dias)' deve ser maior que zero.", ex.Message);
    }

    [Fact]
    public void Create_WithoutDuration_IsAccepted()
    {
        Assert.Null(Create(durationDays: null).DurationDays);
    }

    [Fact]
    public void Create_UsageDeadline_IsStoredWithoutValidation()
    {
        var beforeStart = new DateOnly(2000, 1, 1);

        Assert.Equal(beforeStart, Create(usageDeadline: beforeStart).UsageDeadline);
        Assert.Null(Create(usageDeadline: null).UsageDeadline);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankNotes_StoresNull(string? notes)
    {
        Assert.Null(Create(notes: notes).Notes);
    }
}
