namespace Prescription.Blazor.Models;

/// <summary>Item as filled on the screen (empty text = "", not chosen = null).</summary>
public sealed class PrescriptionItemDraft
{
    /// <summary>Catalog medication (name and form come from the catalog).</summary>
    public uint? MedicationId { get; set; }

    /// <summary>Medication not in the catalog (e.g. compounded): name and form typed.</summary>
    public bool NotRegistered { get; set; }

    public string MedicationName { get; set; } = "";
    public uint? PharmaceuticalFormId { get; set; }
    public decimal? Dose { get; set; }
    public uint? DoseUnitId { get; set; }
    public uint? FrequencyId { get; set; }
    public DateOnly? StartDate { get; set; }
    public int? DurationDays { get; set; }
    public bool IsContinuousUse { get; set; }
    public uint? AdministrationRouteId { get; set; }
    public DateOnly? UsageDeadline { get; set; }
    public string Notes { get; set; } = "";

    public bool IsCatalogMedication => !NotRegistered && MedicationId is not null;

    /// <summary>Back to the search (no medication chosen).</summary>
    public void ClearMedication()
    {
        MedicationId = null;
        NotRegistered = false;
        MedicationName = "";
        PharmaceuticalFormId = null;
        UsageDeadline = null;
    }

    public static PrescriptionItemDraft From(PrescriptionItem item) => new()
    {
        MedicationId = item.MedicationId,
        NotRegistered = item.MedicationId is null,
        MedicationName = item.MedicationName,
        PharmaceuticalFormId = item.PharmaceuticalFormId,
        Dose = item.Dose,
        DoseUnitId = item.DoseUnitId,
        FrequencyId = item.FrequencyId,
        StartDate = item.StartDate,
        DurationDays = item.DurationDays is { } days ? (int)days : null,
        IsContinuousUse = item.IsContinuousUse,
        AdministrationRouteId = item.AdministrationRouteId,
        UsageDeadline = item.UsageDeadline,
        Notes = item.Notes ?? ""
    };
}

/// <summary>Prescription as filled on the create / edit screen.</summary>
public sealed class PrescriptionDraft
{
    public uint? ClientId { get; set; }
    public uint? InstitutionId { get; set; }
    public PrescriptionFormat Format { get; set; } = PrescriptionFormat.Structured;
    public PrescriptionCategory Category { get; set; } = PrescriptionCategory.Common;
    public string FreeText { get; set; } = "";
    public List<PrescriptionItemDraft> Items { get; } = [];

    private static string Required(string field) => $"Campo '{field}' é obrigatório.";
    private static string GreaterThanZero(string field) => $"Campo '{field}' deve ser maior que zero.";

    /// <summary>Same rules and messages as the Domain; the API checks them again.</summary>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        if (ClientId is null)
        {
            errors.Add(Required("Cliente"));
        }
        if (InstitutionId is null)
        {
            errors.Add(Required("Instituição"));
        }

        if (Format == PrescriptionFormat.Unstructured)
        {
            if (string.IsNullOrWhiteSpace(FreeText))
            {
                errors.Add("Receita não estruturada exige texto livre.");
            }
            return errors;
        }

        if (Items.Count == 0)
        {
            errors.Add("Receita estruturada exige no mínimo 1 item.");
        }
        for (var index = 0; index < Items.Count; index++)
        {
            var item = Items[index];
            var prefix = $"Item {index + 1}: ";
            if (item.NotRegistered)
            {
                if (string.IsNullOrWhiteSpace(item.MedicationName))
                {
                    errors.Add(prefix + Required("Medicamento"));
                }
                if (item.PharmaceuticalFormId is null)
                {
                    errors.Add(prefix + Required("Forma farmacêutica"));
                }
            }
            else if (item.MedicationId is null)
            {
                errors.Add(prefix + Required("Medicamento"));
            }
            if (item.Dose is null or <= 0)
            {
                errors.Add(prefix + GreaterThanZero("Dose"));
            }
            if (item.DoseUnitId is null)
            {
                errors.Add(prefix + Required("Unidade"));
            }
            if (item.FrequencyId is null)
            {
                errors.Add(prefix + Required("Frequência"));
            }
            if (item.StartDate is null)
            {
                errors.Add(prefix + Required("Início"));
            }
            if (item.DurationDays is <= 0)
            {
                errors.Add(prefix + GreaterThanZero("Duração (dias)"));
            }
            if (item.AdministrationRouteId is null)
            {
                errors.Add(prefix + Required("Via de administração"));
            }
        }
        return errors;
    }

    /// <summary>Request of the API. Unstructured: no items (ECF.10.01). Call only after <see cref="Validate"/>.</summary>
    public PrescriptionRequest ToRequest() =>
        new(ClientId ?? 0, InstitutionId ?? 0, Format, Category, OptionalText(FreeText),
            Format == PrescriptionFormat.Unstructured ? [] : Items.Select(ToItemRequest).ToList());

    /// <summary>Catalog medication ids of the items (base of the suggested category).</summary>
    public IReadOnlyList<uint> CatalogMedicationIds() =>
        Format == PrescriptionFormat.Unstructured
            ? []
            : Items.Where(item => item.IsCatalogMedication).Select(item => item.MedicationId!.Value).Distinct().ToList();

    public static PrescriptionDraft From(MedicalPrescription prescription)
    {
        var draft = new PrescriptionDraft
        {
            ClientId = prescription.ClientId,
            InstitutionId = prescription.InstitutionId,
            Format = prescription.Format,
            Category = prescription.Category,
            FreeText = prescription.FreeText ?? ""
        };
        draft.Items.AddRange(prescription.Items.Select(PrescriptionItemDraft.From));
        return draft;
    }

    /// <summary>Optional text with only spaces is sent as empty (null).</summary>
    private static string? OptionalText(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static PrescriptionItemRequest ToItemRequest(PrescriptionItemDraft item)
    {
        var registered = item.IsCatalogMedication;
        return new PrescriptionItemRequest(
            registered ? item.MedicationId : null,
            registered ? null : OptionalText(item.MedicationName),
            registered ? null : item.PharmaceuticalFormId,
            item.Dose ?? 0, item.DoseUnitId ?? 0, item.FrequencyId ?? 0, item.StartDate ?? default,
            item.DurationDays is { } days ? (uint)days : null, item.IsContinuousUse, item.AdministrationRouteId ?? 0,
            item.UsageDeadline, OptionalText(item.Notes));
    }
}
