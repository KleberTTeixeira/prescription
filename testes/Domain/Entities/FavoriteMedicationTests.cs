using Prescription.Domain.Entities;
using Prescription.Domain.Exceptions;

namespace Prescription.Tests.Domain.Entities;

public class FavoriteMedicationTests
{
    [Fact]
    public void Create_WithValidData_SetsFieldsAndCreatedAt()
    {
        var favorite = new FavoriteMedication(2, 5, TestData.Now);

        Assert.Equal(2u, favorite.ProfessionalId);
        Assert.Equal(5u, favorite.MedicationId);
        Assert.Equal(TestData.Now, favorite.CreatedAt);
    }

    [Theory]
    [InlineData(0u, 1u, "Profissional")]
    [InlineData(1u, 0u, "Medicamento")]
    public void Create_WithoutRequiredId_Throws(uint professionalId, uint medicationId, string field)
    {
        var ex = Assert.Throws<DomainException>(() =>
            new FavoriteMedication(professionalId, medicationId, TestData.Now));

        Assert.Equal($"Campo '{field}' é obrigatório.", ex.Message);
    }
}
