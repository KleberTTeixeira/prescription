using Prescription.Domain.Entities;
using Prescription.Domain.Exceptions;

namespace Prescription.Tests.Domain.Entities;

public class PrescriptionTemplateTests
{
    [Fact]
    public void Create_WithValidData_SetsFieldsAndCreatedAt()
    {
        var template = new PrescriptionTemplate(1, "Analgesia simples", "Dipirona 500 mg", TestData.Now);

        Assert.Equal(1u, template.ProfessionalId);
        Assert.Equal("Analgesia simples", template.Title);
        Assert.Equal("Dipirona 500 mg", template.Content);
        Assert.Equal(TestData.Now, template.CreatedAt);
        Assert.Null(template.UpdatedAt);
    }

    [Fact]
    public void Create_WithoutProfessional_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => new PrescriptionTemplate(0, "Título", "Texto", TestData.Now));
        Assert.Equal("Campo 'Profissional' é obrigatório.", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutTitle_Throws(string title)
    {
        var ex = Assert.Throws<DomainException>(() => new PrescriptionTemplate(1, title, "Texto", TestData.Now));
        Assert.Equal("Campo 'Título do texto padrão' é obrigatório.", ex.Message);
    }

    [Fact]
    public void Create_WithTitleAboveMax_Throws()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new PrescriptionTemplate(1, TestData.Text(101), "Texto", TestData.Now));

        Assert.Equal("Campo 'Título do texto padrão' deve ter no máximo 100 caracteres.", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutContent_Throws(string content)
    {
        var ex = Assert.Throws<DomainException>(() => new PrescriptionTemplate(1, "Título", content, TestData.Now));
        Assert.Equal("Campo 'Conteúdo do texto padrão' é obrigatório.", ex.Message);
    }

    [Fact]
    public void Update_ChangesTextAndSetsUpdatedAt()
    {
        var template = new PrescriptionTemplate(1, "Título", "Texto", TestData.Now);
        var later = TestData.Now.AddHours(1);

        template.Update("Novo título", "Novo texto", later);

        Assert.Equal("Novo título", template.Title);
        Assert.Equal("Novo texto", template.Content);
        Assert.Equal(later, template.UpdatedAt);
        Assert.Equal(TestData.Now, template.CreatedAt);
    }

    [Fact]
    public void Update_WithInvalidTitle_Throws()
    {
        var template = new PrescriptionTemplate(1, "Título", "Texto", TestData.Now);

        var ex = Assert.Throws<DomainException>(() => template.Update("", "Texto", TestData.Now));
        Assert.Equal("Campo 'Título do texto padrão' é obrigatório.", ex.Message);
    }
}
