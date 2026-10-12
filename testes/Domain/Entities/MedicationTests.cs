using Prescription.Domain.Entities;
using Prescription.Domain.Exceptions;

namespace Prescription.Tests.Domain.Entities;

public class MedicationTests
{
    private static Medication Create(string name = "Amoxicilina", string activeIngredient = "Amoxicilina",
                                     string? concentration = "500 mg", uint formId = 1, uint validityId = 2) =>
        new(name, activeIngredient, concentration, formId, validityId,
            isControlled: false, isAntimicrobial: true, isActive: true);

    [Fact]
    public void Create_WithValidData_SetsFields()
    {
        var medication = new Medication("Clonazepam", "Clonazepam", "2 mg", 1, 4, true, false, true);

        Assert.Equal("Clonazepam", medication.Name);
        Assert.Equal("Clonazepam", medication.ActiveIngredient);
        Assert.Equal("2 mg", medication.Concentration);
        Assert.Equal(1u, medication.PharmaceuticalFormId);
        Assert.Equal(4u, medication.ValidityPeriodId);
        Assert.True(medication.IsControlled);
        Assert.False(medication.IsAntimicrobial);
        Assert.True(medication.IsActive);
    }

    [Fact]
    public void Create_WithoutName_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => Create(name: ""));
        Assert.Equal("Campo 'Nome do medicamento' é obrigatório.", ex.Message);
    }

    [Fact]
    public void Create_WithoutActiveIngredient_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => Create(activeIngredient: ""));
        Assert.Equal("Campo 'Princípio ativo' é obrigatório.", ex.Message);
    }

    [Theory]
    [InlineData(151, 150, 60, "Nome do medicamento", 150)]
    [InlineData(150, 151, 60, "Princípio ativo", 150)]
    [InlineData(150, 150, 61, "Concentração", 60)]
    public void Create_AboveMaxLength_Throws(int name, int ingredient, int concentration, string field, int max)
    {
        var ex = Assert.Throws<DomainException>(() =>
            Create(TestData.Text(name), TestData.Text(ingredient), TestData.Text(concentration)));

        Assert.Equal($"Campo '{field}' deve ter no máximo {max} caracteres.", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankConcentration_StoresNull(string? concentration)
    {
        Assert.Null(Create(concentration: concentration).Concentration);
    }

    [Theory]
    [InlineData(0u, 1u, "Forma farmacêutica")]
    [InlineData(1u, 0u, "Prazo de utilização")]
    public void Create_WithoutRequiredId_Throws(uint formId, uint validityId, string field)
    {
        var ex = Assert.Throws<DomainException>(() => Create(formId: formId, validityId: validityId));
        Assert.Equal($"Campo '{field}' é obrigatório.", ex.Message);
    }
}
