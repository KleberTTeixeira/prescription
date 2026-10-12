using Prescription.Domain.Entities;
using Prescription.Domain.Exceptions;

namespace Prescription.Tests.Domain.Entities;

public class ProfessionalTests
{
    [Fact]
    public void Create_WithValidData_SetsFields()
    {
        var professional = new Professional("Dr. Carlos Lima", "CRM", "123456", "SP");

        Assert.Equal("Dr. Carlos Lima", professional.FullName);
        Assert.Equal("CRM", professional.Council);
        Assert.Equal("123456", professional.CouncilNumber);
        Assert.Equal("SP", professional.CouncilState);
    }

    [Theory]
    [InlineData("", "CRM", "1", "SP", "Nome do profissional")]
    [InlineData("Ana", "", "1", "SP", "Conselho")]
    [InlineData("Ana", "CRM", "", "SP", "Número do conselho")]
    [InlineData("Ana", "CRM", "1", "", "UF do conselho")]
    public void Create_WithoutRequiredField_Throws(string name, string council, string number, string state,
                                                  string field)
    {
        var ex = Assert.Throws<DomainException>(() => new Professional(name, council, number, state));
        Assert.Equal($"Campo '{field}' é obrigatório.", ex.Message);
    }

    [Theory]
    [InlineData(151, 10, 20, 2, "Nome do profissional", 150)]
    [InlineData(150, 11, 20, 2, "Conselho", 10)]
    [InlineData(150, 10, 21, 2, "Número do conselho", 20)]
    [InlineData(150, 10, 20, 3, "UF do conselho", 2)]
    public void Create_AboveMaxLength_Throws(int name, int council, int number, int state, string field, int max)
    {
        var ex = Assert.Throws<DomainException>(() =>
            new Professional(TestData.Text(name), TestData.Text(council), TestData.Text(number),
                             TestData.Text(state)));

        Assert.Equal($"Campo '{field}' deve ter no máximo {max} caracteres.", ex.Message);
    }
}
