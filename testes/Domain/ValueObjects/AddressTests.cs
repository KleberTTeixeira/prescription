using Prescription.Domain.Exceptions;
using Prescription.Domain.ValueObjects;

namespace Prescription.Tests.Domain.ValueObjects;

public class AddressTests
{
    private static Address Create(string street = "Rua A", string number = "10", string? complement = null,
                                  string district = "Centro", string city = "São Paulo",
                                  string state = "SP", string zipCode = "01001000") =>
        new(street, number, complement, district, city, state, zipCode);

    [Fact]
    public void Create_WithValidData_SetsAllFields()
    {
        var address = TestData.ValidAddress();

        Assert.Equal("Rua das Flores", address.Street);
        Assert.Equal("100", address.Number);
        Assert.Equal("Sala 2", address.Complement);
        Assert.Equal("Centro", address.District);
        Assert.Equal("São Paulo", address.City);
        Assert.Equal("SP", address.State);
        Assert.Equal("01001000", address.ZipCode);
    }

    [Fact]
    public void Addresses_WithSameValues_AreEqual()
    {
        Assert.Equal(TestData.ValidAddress(), TestData.ValidAddress());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutStreet_Throws(string? street)
    {
        var ex = Assert.Throws<DomainException>(() => Create(street: street!));
        Assert.Equal("Campo 'Logradouro' é obrigatório.", ex.Message);
    }

    [Fact]
    public void Create_WithoutNumber_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => Create(number: ""));
        Assert.Equal("Campo 'Número' é obrigatório.", ex.Message);
    }

    [Fact]
    public void Create_WithoutDistrict_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => Create(district: ""));
        Assert.Equal("Campo 'Bairro' é obrigatório.", ex.Message);
    }

    [Fact]
    public void Create_WithoutCity_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => Create(city: ""));
        Assert.Equal("Campo 'Cidade' é obrigatório.", ex.Message);
    }

    [Fact]
    public void Create_WithoutState_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => Create(state: ""));
        Assert.Equal("Campo 'UF' é obrigatório.", ex.Message);
    }

    [Theory]
    [InlineData(151, 10, 60, 80, 80, 2, "Logradouro", 150)]
    [InlineData(150, 11, 60, 80, 80, 2, "Número", 10)]
    [InlineData(150, 10, 61, 80, 80, 2, "Complemento", 60)]
    [InlineData(150, 10, 60, 81, 80, 2, "Bairro", 80)]
    [InlineData(150, 10, 60, 80, 81, 2, "Cidade", 80)]
    [InlineData(150, 10, 60, 80, 80, 3, "UF", 2)]
    public void Create_AboveMaxLength_Throws(int street, int number, int complement, int district,
                                             int city, int state, string field, int max)
    {
        var ex = Assert.Throws<DomainException>(() =>
            Create(TestData.Text(street), TestData.Text(number), TestData.Text(complement),
                   TestData.Text(district), TestData.Text(city), TestData.Text(state)));

        Assert.Equal($"Campo '{field}' deve ter no máximo {max} caracteres.", ex.Message);
    }

    [Fact]
    public void Create_AtMaxLength_IsAccepted()
    {
        var address = Create(TestData.Text(150), TestData.Text(10), TestData.Text(60),
                             TestData.Text(80), TestData.Text(80), TestData.Text(2));

        Assert.Equal(150, address.Street.Length);
    }

    [Theory]
    [InlineData("0100100")]
    [InlineData("010010000")]
    [InlineData("01001-00")]
    [InlineData("0100100a")]
    public void Create_WithInvalidZipCode_Throws(string zipCode)
    {
        var ex = Assert.Throws<DomainException>(() => Create(zipCode: zipCode));
        Assert.Equal("Campo 'CEP' deve ter 8 dígitos.", ex.Message);
    }

    [Fact]
    public void Create_WithoutZipCode_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => Create(zipCode: ""));
        Assert.Equal("Campo 'CEP' é obrigatório.", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankComplement_StoresNull(string? complement)
    {
        Assert.Null(Create(complement: complement).Complement);
    }
}
