using Prescription.Domain.Enums;
using Prescription.Domain.Exceptions;

namespace Prescription.Domain.Entities;

/// <summary>
/// Table prescriptions. Aggregate root: items are changed only through this class.
/// Unstructured (ECF.10.01): free text required, no items.
/// Structured (ECF.10.03): at least one item, free text optional (general note).
/// Issued = first confirmed print (PrintedAt filled); an issued prescription is locked.
/// </summary>
public sealed class MedicalPrescription
{
    private readonly List<PrescriptionItem> _items = [];

    public uint Id { get; private set; }
    public uint ClientId { get; private set; }
    public uint ProfessionalId { get; private set; }
    public uint InstitutionId { get; private set; }
    public PrescriptionFormat Format { get; private set; }
    public PrescriptionCategory Category { get; private set; }
    public string? FreeText { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public DateTime? PrintedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public bool IsIssued => PrintedAt.HasValue;

    public Client Client { get; private set; } = null!;
    public Professional Professional { get; private set; } = null!;
    public Institution Institution { get; private set; } = null!;
    public IReadOnlyCollection<PrescriptionItem> Items => _items.AsReadOnly();

    private MedicalPrescription() { }

    public static MedicalPrescription Create(uint clientId, uint professionalId, uint institutionId,
                                             PrescriptionFormat format, PrescriptionCategory category,
                                             string? freeText, IEnumerable<PrescriptionItem>? items,
                                             DateTime now)
    {
        var prescription = new MedicalPrescription { IssuedAt = now, CreatedAt = now };
        prescription.Apply(clientId, professionalId, institutionId, format, category, freeText, items);
        return prescription;
    }

    public void Update(uint clientId, uint professionalId, uint institutionId,
                       PrescriptionFormat format, PrescriptionCategory category,
                       string? freeText, IEnumerable<PrescriptionItem>? items, DateTime now)
    {
        EnsureNotIssued();
        Apply(clientId, professionalId, institutionId, format, category, freeText, items);
        UpdatedAt = now;
    }

    public void AddItem(PrescriptionItem item, DateTime now)
    {
        EnsureNotIssued();
        ArgumentNullException.ThrowIfNull(item);

        if (Format == PrescriptionFormat.Unstructured)
            throw new DomainException("Receita não estruturada não pode ter itens.");

        _items.Add(item);
        UpdatedAt = now;
    }

    public void RemoveItem(PrescriptionItem item, DateTime now)
    {
        EnsureNotIssued();

        if (!_items.Contains(item))
            throw new DomainException("Item não pertence a esta receita.");

        if (Format == PrescriptionFormat.Structured && _items.Count == 1)
            throw new DomainException("Receita estruturada exige no mínimo 1 item.");

        _items.Remove(item);
        UpdatedAt = now;
    }

    public void ReplaceItems(IEnumerable<PrescriptionItem>? items, DateTime now)
    {
        EnsureNotIssued();
        var list = ValidateItems(Format, items);
        _items.Clear();
        _items.AddRange(list);
        UpdatedAt = now;
    }

    /// <summary>Records the first confirmed print (issue). Reprints keep the original date.</summary>
    public void MarkAsPrinted(DateTime now)
    {
        if (IsIssued)
            return;

        PrintedAt = now;
        UpdatedAt = now;
    }

    private void Apply(uint clientId, uint professionalId, uint institutionId,
                       PrescriptionFormat format, PrescriptionCategory category,
                       string? freeText, IEnumerable<PrescriptionItem>? items)
    {
        // Validate everything before changing any state.
        var validClientId = Guard.Id(clientId, "Cliente");
        var validProfessionalId = Guard.Id(professionalId, "Profissional");
        var validInstitutionId = Guard.Id(institutionId, "Instituição");
        var validFormat = Guard.Defined(format, "Formato da receita");
        var validCategory = Guard.Defined(category, "Categoria da receita");

        var text = Guard.OptionalText(freeText);
        if (validFormat == PrescriptionFormat.Unstructured && text is null)
            throw new DomainException("Receita não estruturada exige texto livre.");

        var list = ValidateItems(validFormat, items);

        ClientId = validClientId;
        ProfessionalId = validProfessionalId;
        InstitutionId = validInstitutionId;
        Format = validFormat;
        Category = validCategory;
        FreeText = text;
        _items.Clear();
        _items.AddRange(list);
    }

    private static List<PrescriptionItem> ValidateItems(PrescriptionFormat format,
                                                        IEnumerable<PrescriptionItem>? items)
    {
        var list = items?.ToList() ?? [];

        if (list.Any(item => item is null))
            throw new DomainException("Item da receita inválido.");

        if (format == PrescriptionFormat.Unstructured && list.Count > 0)
            throw new DomainException("Receita não estruturada não pode ter itens.");

        if (format == PrescriptionFormat.Structured && list.Count == 0)
            throw new DomainException("Receita estruturada exige no mínimo 1 item.");

        return list;
    }

    private void EnsureNotIssued()
    {
        if (IsIssued)
            throw new DomainException("Receita já emitida (impressa) não pode ser alterada.");
    }
}
