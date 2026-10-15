using Prescription.Blazor.Models;

namespace Prescription.Blazor.Tests.Models;

public class PrescriptionDraftTests
{
    private static PrescriptionItemDraft CatalogItem(uint medicationId = 5) => new()
    {
        MedicationId = medicationId, MedicationName = "Amoxicilina 500 mg", PharmaceuticalFormId = 2, Dose = 1,
        DoseUnitId = 7, FrequencyId = 3, StartDate = new DateOnly(2026, 10, 15), AdministrationRouteId = 1
    };

    private static PrescriptionDraft Valid()
    {
        var draft = new PrescriptionDraft { ClientId = 1, InstitutionId = 1 };
        draft.Items.Add(CatalogItem());
        return draft;
    }

    [Fact]
    public void Valid_NoErrors() => Assert.Empty(Valid().Validate());

    [Fact]
    public void ClientAndInstitution_Required()
    {
        var draft = Valid();
        draft.ClientId = null;
        draft.InstitutionId = null;

        Assert.Equal(["Campo 'Cliente' é obrigatório.", "Campo 'Instituição' é obrigatório."], draft.Validate());
    }

    [Fact]
    public void Unstructured_RequiresFreeText_IgnoresItems()
    {
        var draft = Valid();
        draft.Format = PrescriptionFormat.Unstructured;
        draft.FreeText = "   ";
        draft.Items[0].Dose = null;

        Assert.Equal(["Receita não estruturada exige texto livre."], draft.Validate());
    }

    [Fact]
    public void Structured_RequiresOneItem()
    {
        var draft = Valid();
        draft.Items.Clear();

        Assert.Equal(["Receita estruturada exige no mínimo 1 item."], draft.Validate());
    }

    [Fact]
    public void Item_EmptyFields_AllMessagesWithItemNumber()
    {
        var draft = Valid();
        draft.Items.Add(new PrescriptionItemDraft { DurationDays = 0 });

        Assert.Equal(
        [
            "Item 2: Campo 'Medicamento' é obrigatório.",
            "Item 2: Campo 'Dose' deve ser maior que zero.",
            "Item 2: Campo 'Unidade' é obrigatório.",
            "Item 2: Campo 'Frequência' é obrigatório.",
            "Item 2: Campo 'Início' é obrigatório.",
            "Item 2: Campo 'Duração (dias)' deve ser maior que zero.",
            "Item 2: Campo 'Via de administração' é obrigatório."
        ], draft.Validate());
    }

    [Fact]
    public void NotRegistered_RequiresNameAndForm()
    {
        var draft = Valid();
        draft.Items[0].ClearMedication();
        draft.Items[0].NotRegistered = true;
        draft.Items[0].MedicationName = "  ";

        Assert.Equal(["Item 1: Campo 'Medicamento' é obrigatório.", "Item 1: Campo 'Forma farmacêutica' é obrigatório."],
                     draft.Validate());
    }

    [Fact]
    public void ToRequest_CatalogMedication_SendsOnlyId_OptionalTextsAsNull()
    {
        var draft = Valid();
        draft.FreeText = "  ";
        draft.Items[0].Notes = " ";
        draft.Items[0].DurationDays = 7;
        draft.Items[0].UsageDeadline = new DateOnly(2026, 10, 25);

        var request = draft.ToRequest();

        Assert.Null(request.FreeText);
        var item = Assert.Single(request.Items);
        Assert.Equal(new PrescriptionItemRequest(5, null, null, 1, 7, 3, new DateOnly(2026, 10, 15), 7, false, 1,
                                                 new DateOnly(2026, 10, 25), null), item);
    }

    [Fact]
    public void ToRequest_NotRegistered_SendsTrimmedNameAndForm()
    {
        var draft = Valid();
        draft.Items[0].ClearMedication();
        draft.Items[0].NotRegistered = true;
        draft.Items[0].MedicationName = "  Fórmula manipulada ";
        draft.Items[0].PharmaceuticalFormId = 4;

        var item = Assert.Single(draft.ToRequest().Items);

        Assert.Null(item.MedicationId);
        Assert.Equal("Fórmula manipulada", item.MedicationName);
        Assert.Equal(4u, item.PharmaceuticalFormId);
    }

    [Fact]
    public void ToRequest_Unstructured_NoItems()
    {
        var draft = Valid();
        draft.Format = PrescriptionFormat.Unstructured;
        draft.FreeText = " Texto ";

        var request = draft.ToRequest();

        Assert.Empty(request.Items);
        Assert.Equal("Texto", request.FreeText);
    }

    [Fact]
    public void CatalogMedicationIds_DistinctCatalogOnly_EmptyWhenUnstructured()
    {
        var draft = Valid();
        draft.Items.Add(CatalogItem(5));
        draft.Items.Add(CatalogItem(16));
        draft.Items.Add(new PrescriptionItemDraft { NotRegistered = true, MedicationName = "Manipulado" });

        Assert.Equal([5u, 16u], draft.CatalogMedicationIds());

        draft.Format = PrescriptionFormat.Unstructured;
        Assert.Empty(draft.CatalogMedicationIds());
    }

    [Fact]
    public void From_LoadsPrescriptionAndItems()
    {
        var prescription = new MedicalPrescription(3, 1, "Maria", 1, "Ana", 2, "Clínica", PrescriptionFormat.Structured,
            PrescriptionCategory.Antimicrobial, null, new DateTime(2026, 10, 5, 9, 0, 0), null, false,
            [
                new PrescriptionItem(9, null, "Manipulado", 4, "Solução", 10, 7, "ml", 3, "8/8 horas",
                                     new DateOnly(2026, 10, 5), 30, true, 1, "Oral", null, "Agitar")
            ]);

        var draft = PrescriptionDraft.From(prescription);

        Assert.Equal((1u, 2u, PrescriptionCategory.Antimicrobial, ""),
                     (draft.ClientId!.Value, draft.InstitutionId!.Value, draft.Category, draft.FreeText));
        var item = Assert.Single(draft.Items);
        Assert.True(item.NotRegistered);
        Assert.Equal(("Manipulado", 4u, 30, true, "Agitar"),
                     (item.MedicationName, item.PharmaceuticalFormId!.Value, item.DurationDays!.Value,
                      item.IsContinuousUse, item.Notes));
    }
}
