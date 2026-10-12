using Prescription.Domain.Entities;
using Prescription.Domain.Enums;
using Prescription.Domain.Exceptions;

namespace Prescription.Tests.Domain.Entities;

public class ClientTests
{
    [Fact]
    public void Create_WithValidData_SetsFields()
    {
        var address = TestData.ValidAddress();

        var client = new Client("Maria Souza", new DateOnly(1980, 5, 20), Sex.Female, address);

        Assert.Equal("Maria Souza", client.FullName);
        Assert.Equal(new DateOnly(1980, 5, 20), client.BirthDate);
        Assert.Equal(Sex.Female, client.Sex);
        Assert.Equal(address, client.Address);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutName_Throws(string name)
    {
        var ex = Assert.Throws<DomainException>(() =>
            new Client(name, new DateOnly(1980, 1, 1), Sex.Male, TestData.ValidAddress()));

        Assert.Equal("Campo 'Nome do cliente' é obrigatório.", ex.Message);
    }

    [Fact]
    public void Create_WithNameAboveMax_Throws()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new Client(TestData.Text(151), new DateOnly(1980, 1, 1), Sex.Male, TestData.ValidAddress()));

        Assert.Equal("Campo 'Nome do cliente' deve ter no máximo 150 caracteres.", ex.Message);
    }

    [Fact]
    public void Create_WithInvalidSex_Throws()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new Client("João", new DateOnly(1980, 1, 1), (Sex)9, TestData.ValidAddress()));

        Assert.Equal("Campo 'Sexo' possui valor inválido.", ex.Message);
    }

    [Fact]
    public void Create_WithoutAddress_Throws()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new Client("João", new DateOnly(1980, 1, 1), Sex.Male, null!));

        Assert.Equal("Campo 'Endereço do cliente' é obrigatório.", ex.Message);
    }
}
