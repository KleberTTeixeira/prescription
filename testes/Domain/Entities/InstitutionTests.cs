using Prescription.Domain.Entities;
using Prescription.Domain.Exceptions;

namespace Prescription.Tests.Domain.Entities;

public class InstitutionTests
{
    private static Institution Create(string name = "Clínica Central", string cnes = "1234567",
                                      string phone = "1133334444") =>
        new(name, cnes, TestData.ValidAddress(), phone);

    [Fact]
    public void Create_WithValidData_SetsFields()
    {
        var institution = Create();

        Assert.Equal("Clínica Central", institution.Name);
        Assert.Equal("1234567", institution.Cnes);
        Assert.Equal(TestData.ValidAddress(), institution.Address);
        Assert.Equal("1133334444", institution.Phone);
    }

    [Fact]
    public void Create_WithoutName_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => Create(name: ""));
        Assert.Equal("Campo 'Nome da instituição' é obrigatório.", ex.Message);
    }

    [Fact]
    public void Create_WithNameAboveMax_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => Create(name: TestData.Text(151)));
        Assert.Equal("Campo 'Nome da instituição' deve ter no máximo 150 caracteres.", ex.Message);
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("12345678")]
    [InlineData("12345a7")]
    public void Create_WithInvalidCnes_Throws(string cnes)
    {
        var ex = Assert.Throws<DomainException>(() => Create(cnes: cnes));
        Assert.Equal("Campo 'CNES' deve ter 7 dígitos.", ex.Message);
    }

    [Fact]
    public void Create_WithoutCnes_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => Create(cnes: ""));
        Assert.Equal("Campo 'CNES' é obrigatório.", ex.Message);
    }

    [Fact]
    public void Create_WithoutPhone_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => Create(phone: ""));
        Assert.Equal("Campo 'Telefone da instituição' é obrigatório.", ex.Message);
    }

    [Fact]
    public void Create_WithPhoneAboveMax_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => Create(phone: TestData.Text(21)));
        Assert.Equal("Campo 'Telefone da instituição' deve ter no máximo 20 caracteres.", ex.Message);
    }

    [Fact]
    public void Create_WithoutAddress_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => new Institution("Clínica", "1234567", null!, "11"));
        Assert.Equal("Campo 'Endereço da instituição' é obrigatório.", ex.Message);
    }
}
