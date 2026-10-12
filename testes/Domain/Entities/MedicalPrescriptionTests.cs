using Prescription.Domain.Entities;
using Prescription.Domain.Enums;
using Prescription.Domain.Exceptions;

namespace Prescription.Tests.Domain.Entities;

public class MedicalPrescriptionTests
{
    private static readonly DateTime Later = TestData.Now.AddHours(1);

    private static MedicalPrescription CreateUnstructured(string? freeText = "Dipirona 500 mg de 6/6h") =>
        MedicalPrescription.Create(1, 1, 1, PrescriptionFormat.Unstructured, PrescriptionCategory.Common,
                                   freeText, items: null, TestData.Now);

    private static MedicalPrescription CreateStructured(IEnumerable<PrescriptionItem>? items = null,
                                                        string? freeText = null) =>
        MedicalPrescription.Create(1, 1, 1, PrescriptionFormat.Structured, PrescriptionCategory.Antimicrobial,
                                   freeText, items ?? [TestData.ValidItem()], TestData.Now);

    // ---- Create ----

    [Fact]
    public void Create_SetsIssuedAtAndCreatedAtToNow_AndIsNotIssued()
    {
        var prescription = CreateStructured();

        Assert.Equal(TestData.Now, prescription.IssuedAt);
        Assert.Equal(TestData.Now, prescription.CreatedAt);
        Assert.Null(prescription.UpdatedAt);
        Assert.Null(prescription.PrintedAt);
        Assert.False(prescription.IsIssued);
    }

    [Fact]
    public void Create_SetsAllFields()
    {
        var prescription = MedicalPrescription.Create(2, 3, 4, PrescriptionFormat.Structured,
            PrescriptionCategory.SpecialControl, "Observação", [TestData.ValidItem()], TestData.Now);

        Assert.Equal(2u, prescription.ClientId);
        Assert.Equal(3u, prescription.ProfessionalId);
        Assert.Equal(4u, prescription.InstitutionId);
        Assert.Equal(PrescriptionFormat.Structured, prescription.Format);
        Assert.Equal(PrescriptionCategory.SpecialControl, prescription.Category);
        Assert.Equal("Observação", prescription.FreeText);
        Assert.Single(prescription.Items);
    }

    [Theory]
    [InlineData(0u, 1u, 1u, "Cliente")]
    [InlineData(1u, 0u, 1u, "Profissional")]
    [InlineData(1u, 1u, 0u, "Instituição")]
    public void Create_WithoutRequiredId_Throws(uint clientId, uint professionalId, uint institutionId, string field)
    {
        var ex = Assert.Throws<DomainException>(() =>
            MedicalPrescription.Create(clientId, professionalId, institutionId, PrescriptionFormat.Unstructured,
                                       PrescriptionCategory.Common, "Texto", null, TestData.Now));

        Assert.Equal($"Campo '{field}' é obrigatório.", ex.Message);
    }

    [Fact]
    public void Create_WithInvalidFormat_Throws()
    {
        var ex = Assert.Throws<DomainException>(() =>
            MedicalPrescription.Create(1, 1, 1, (PrescriptionFormat)99, PrescriptionCategory.Common,
                                       "Texto", null, TestData.Now));

        Assert.Equal("Campo 'Formato da receita' possui valor inválido.", ex.Message);
    }

    [Fact]
    public void Create_WithInvalidCategory_Throws()
    {
        var ex = Assert.Throws<DomainException>(() =>
            MedicalPrescription.Create(1, 1, 1, PrescriptionFormat.Unstructured, (PrescriptionCategory)99,
                                       "Texto", null, TestData.Now));

        Assert.Equal("Campo 'Categoria da receita' possui valor inválido.", ex.Message);
    }

    // ---- ECF.10.01 – unstructured ----

    [Fact]
    public void Unstructured_WithFreeText_IsCreated()
    {
        var prescription = CreateUnstructured("Paracetamol 750 mg");

        Assert.Equal("Paracetamol 750 mg", prescription.FreeText);
        Assert.Empty(prescription.Items);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Unstructured_WithoutFreeText_Throws(string? freeText)
    {
        var ex = Assert.Throws<DomainException>(() => CreateUnstructured(freeText));
        Assert.Equal("Receita não estruturada exige texto livre.", ex.Message);
    }

    [Fact]
    public void Unstructured_WithItems_Throws()
    {
        var ex = Assert.Throws<DomainException>(() =>
            MedicalPrescription.Create(1, 1, 1, PrescriptionFormat.Unstructured, PrescriptionCategory.Common,
                                       "Texto", [TestData.ValidItem()], TestData.Now));

        Assert.Equal("Receita não estruturada não pode ter itens.", ex.Message);
    }

    // ---- ECF.10.03 – structured ----

    [Fact]
    public void Structured_WithItems_IsCreated()
    {
        var prescription = CreateStructured([TestData.ValidItem("A"), TestData.ValidItem("B")]);

        Assert.Equal(2, prescription.Items.Count);
    }

    [Fact]
    public void Structured_WithoutItems_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => CreateStructured([]));
        Assert.Equal("Receita estruturada exige no mínimo 1 item.", ex.Message);
    }

    [Fact]
    public void Structured_WithNullItems_Throws()
    {
        var ex = Assert.Throws<DomainException>(() =>
            MedicalPrescription.Create(1, 1, 1, PrescriptionFormat.Structured, PrescriptionCategory.Common,
                                       null, null, TestData.Now));

        Assert.Equal("Receita estruturada exige no mínimo 1 item.", ex.Message);
    }

    [Fact]
    public void Structured_WithNullItemInList_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => CreateStructured([TestData.ValidItem(), null!]));
        Assert.Equal("Item da receita inválido.", ex.Message);
    }

    [Fact]
    public void Structured_WithFreeText_KeepsItAsGeneralNote()
    {
        Assert.Equal("Retornar em 7 dias", CreateStructured(freeText: "Retornar em 7 dias").FreeText);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Structured_WithBlankFreeText_StoresNull(string? freeText)
    {
        Assert.Null(CreateStructured(freeText: freeText).FreeText);
    }

    [Fact]
    public void Structured_AcceptsMedicationNotInCatalog()
    {
        var item = new PrescriptionItem(null, "Cápsula manipulada de vitamina D3", 1, 1m, 1, 1,
                                        new DateOnly(2026, 10, 11), null, false, 1, null, null);

        var prescription = CreateStructured([item]);

        Assert.Null(Assert.Single(prescription.Items).MedicationId);
    }

    // ---- Update ----

    [Fact]
    public void Update_ChangesFields_AndSetsUpdatedAt_KeepingIssuedAt()
    {
        var prescription = CreateUnstructured();

        prescription.Update(5, 6, 7, PrescriptionFormat.Unstructured, PrescriptionCategory.Antimicrobial,
                            "Novo texto", null, Later);

        Assert.Equal(5u, prescription.ClientId);
        Assert.Equal(6u, prescription.ProfessionalId);
        Assert.Equal(7u, prescription.InstitutionId);
        Assert.Equal(PrescriptionCategory.Antimicrobial, prescription.Category);
        Assert.Equal("Novo texto", prescription.FreeText);
        Assert.Equal(Later, prescription.UpdatedAt);
        Assert.Equal(TestData.Now, prescription.IssuedAt);
    }

    [Fact]
    public void Update_FromUnstructuredToStructured_IsAllowedBeforeIssue()
    {
        var prescription = CreateUnstructured();

        prescription.Update(1, 1, 1, PrescriptionFormat.Structured, PrescriptionCategory.Common,
                            null, [TestData.ValidItem()], Later);

        Assert.Equal(PrescriptionFormat.Structured, prescription.Format);
        Assert.Single(prescription.Items);
        Assert.Null(prescription.FreeText);
    }

    [Fact]
    public void Update_FromStructuredToUnstructured_IsAllowedBeforeIssue()
    {
        var prescription = CreateStructured();

        prescription.Update(1, 1, 1, PrescriptionFormat.Unstructured, PrescriptionCategory.Common,
                            "Texto livre", null, Later);

        Assert.Equal(PrescriptionFormat.Unstructured, prescription.Format);
        Assert.Empty(prescription.Items);
        Assert.Equal("Texto livre", prescription.FreeText);
    }

    [Fact]
    public void Update_ToUnstructuredKeepingItems_Throws()
    {
        var prescription = CreateStructured();

        var ex = Assert.Throws<DomainException>(() =>
            prescription.Update(1, 1, 1, PrescriptionFormat.Unstructured, PrescriptionCategory.Common,
                                "Texto", prescription.Items, Later));

        Assert.Equal("Receita não estruturada não pode ter itens.", ex.Message);
    }

    [Fact]
    public void Update_WithInvalidData_LeavesPrescriptionUnchanged()
    {
        var prescription = CreateStructured();
        var originalItem = Assert.Single(prescription.Items);

        Assert.Throws<DomainException>(() =>
            prescription.Update(9, 9, 9, PrescriptionFormat.Structured, PrescriptionCategory.Common,
                                "Texto", [], Later));

        Assert.Equal(1u, prescription.ClientId);
        Assert.Equal(1u, prescription.ProfessionalId);
        Assert.Equal(1u, prescription.InstitutionId);
        Assert.Equal(PrescriptionCategory.Antimicrobial, prescription.Category);
        Assert.Null(prescription.FreeText);
        Assert.Same(originalItem, Assert.Single(prescription.Items));
        Assert.Null(prescription.UpdatedAt);
    }

    // ---- Items ----

    [Fact]
    public void AddItem_OnStructured_AddsAndSetsUpdatedAt()
    {
        var prescription = CreateStructured();

        prescription.AddItem(TestData.ValidItem("B"), Later);

        Assert.Equal(2, prescription.Items.Count);
        Assert.Equal(Later, prescription.UpdatedAt);
    }

    [Fact]
    public void AddItem_OnUnstructured_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => CreateUnstructured().AddItem(TestData.ValidItem(), Later));
        Assert.Equal("Receita não estruturada não pode ter itens.", ex.Message);
    }

    [Fact]
    public void AddItem_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => CreateStructured().AddItem(null!, Later));
    }

    [Fact]
    public void RemoveItem_WithMoreThanOneItem_Removes()
    {
        var first = TestData.ValidItem("A");
        var prescription = CreateStructured([first, TestData.ValidItem("B")]);

        prescription.RemoveItem(first, Later);

        Assert.Equal("B", Assert.Single(prescription.Items).MedicationName);
        Assert.Equal(Later, prescription.UpdatedAt);
    }

    [Fact]
    public void RemoveItem_LastItemOfStructured_Throws()
    {
        var prescription = CreateStructured();

        var ex = Assert.Throws<DomainException>(() =>
            prescription.RemoveItem(prescription.Items.First(), Later));

        Assert.Equal("Receita estruturada exige no mínimo 1 item.", ex.Message);
    }

    [Fact]
    public void RemoveItem_NotInPrescription_Throws()
    {
        var ex = Assert.Throws<DomainException>(() =>
            CreateStructured().RemoveItem(TestData.ValidItem("Outro"), Later));

        Assert.Equal("Item não pertence a esta receita.", ex.Message);
    }

    [Fact]
    public void ReplaceItems_ReplacesAll()
    {
        var prescription = CreateStructured();

        prescription.ReplaceItems([TestData.ValidItem("X"), TestData.ValidItem("Y")], Later);

        Assert.Equal(["X", "Y"], prescription.Items.Select(i => i.MedicationName));
        Assert.Equal(Later, prescription.UpdatedAt);
    }

    [Fact]
    public void ReplaceItems_WithEmptyOnStructured_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => CreateStructured().ReplaceItems([], Later));
        Assert.Equal("Receita estruturada exige no mínimo 1 item.", ex.Message);
    }

    [Fact]
    public void Items_CannotBeChangedFromOutside()
    {
        var items = CreateStructured().Items;

        Assert.IsNotType<List<PrescriptionItem>>(items);
        Assert.Throws<NotSupportedException>(() => ((ICollection<PrescriptionItem>)items).Add(TestData.ValidItem()));
    }

    // ---- Issue (first confirmed print) ----

    [Fact]
    public void MarkAsPrinted_FirstTime_SetsPrintedAtAndIssues()
    {
        var prescription = CreateStructured();

        prescription.MarkAsPrinted(Later);

        Assert.Equal(Later, prescription.PrintedAt);
        Assert.Equal(Later, prescription.UpdatedAt);
        Assert.True(prescription.IsIssued);
    }

    [Fact]
    public void MarkAsPrinted_AgainKeepsFirstDate()
    {
        var prescription = CreateStructured();
        prescription.MarkAsPrinted(Later);

        prescription.MarkAsPrinted(Later.AddDays(1));

        Assert.Equal(Later, prescription.PrintedAt);
        Assert.Equal(Later, prescription.UpdatedAt);
    }

    [Fact]
    public void IssuedPrescription_CannotBeUpdated()
    {
        var prescription = CreateUnstructured();
        prescription.MarkAsPrinted(Later);

        var ex = Assert.Throws<DomainException>(() =>
            prescription.Update(1, 1, 1, PrescriptionFormat.Unstructured, PrescriptionCategory.Common,
                                "Outro texto", null, Later));

        Assert.Equal("Receita já emitida (impressa) não pode ser alterada.", ex.Message);
        Assert.Equal("Dipirona 500 mg de 6/6h", prescription.FreeText);
    }

    [Fact]
    public void IssuedPrescription_CannotAddItem()
    {
        var prescription = CreateStructured();
        prescription.MarkAsPrinted(Later);

        var ex = Assert.Throws<DomainException>(() => prescription.AddItem(TestData.ValidItem(), Later));
        Assert.Equal("Receita já emitida (impressa) não pode ser alterada.", ex.Message);
    }

    [Fact]
    public void IssuedPrescription_CannotRemoveItem()
    {
        var prescription = CreateStructured([TestData.ValidItem("A"), TestData.ValidItem("B")]);
        prescription.MarkAsPrinted(Later);

        var ex = Assert.Throws<DomainException>(() =>
            prescription.RemoveItem(prescription.Items.First(), Later));

        Assert.Equal("Receita já emitida (impressa) não pode ser alterada.", ex.Message);
    }

    [Fact]
    public void IssuedPrescription_CannotReplaceItems()
    {
        var prescription = CreateStructured();
        prescription.MarkAsPrinted(Later);

        var ex = Assert.Throws<DomainException>(() =>
            prescription.ReplaceItems([TestData.ValidItem("X")], Later));

        Assert.Equal("Receita já emitida (impressa) não pode ser alterada.", ex.Message);
    }
}
