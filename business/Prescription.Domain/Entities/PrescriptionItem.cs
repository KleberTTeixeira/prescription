using Prescription.Domain.Exceptions;

namespace Prescription.Domain.Entities;

/// <summary>
/// Table prescription_items – item of a structured prescription (ECF.10.03 c, d).
/// MedicationId is null for medications not in the catalog (e.g. compounded);
/// MedicationName is always stored. Changed only through <see cref="MedicalPrescription"/>.
/// </summary>
public sealed class PrescriptionItem
{
    private const decimal MaxDose = 9_999_999.999m; // DECIMAL(10,3)

    public uint Id { get; private set; }
    public uint PrescriptionId { get; private set; }
    public uint? MedicationId { get; private set; }
    public string MedicationName { get; private set; } = string.Empty;
    public uint PharmaceuticalFormId { get; private set; }
    public decimal Dose { get; private set; }
    public uint DoseUnitId { get; private set; }
    public uint FrequencyId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public uint? DurationDays { get; private set; }
    public bool IsContinuousUse { get; private set; }
    public uint AdministrationRouteId { get; private set; }
    public DateOnly? UsageDeadline { get; private set; }
    public string? Notes { get; private set; }

    public Medication? Medication { get; private set; }
    public PharmaceuticalForm PharmaceuticalForm { get; private set; } = null!;
    public DoseUnit DoseUnit { get; private set; } = null!;
    public Frequency Frequency { get; private set; } = null!;
    public AdministrationRoute AdministrationRoute { get; private set; } = null!;

    private PrescriptionItem() { }

    public PrescriptionItem(uint? medicationId, string medicationName, uint pharmaceuticalFormId,
                            decimal dose, uint doseUnitId, uint frequencyId, DateOnly startDate,
                            uint? durationDays, bool isContinuousUse, uint administrationRouteId,
                            DateOnly? usageDeadline, string? notes)
    {
        MedicationId = Guard.OptionalPositive(medicationId, "Medicamento");
        MedicationName = Guard.Required(medicationName, 150, "Nome do medicamento");
        PharmaceuticalFormId = Guard.Id(pharmaceuticalFormId, "Forma farmacêutica");
        Dose = ValidateDose(dose);
        DoseUnitId = Guard.Id(doseUnitId, "Unidade de dose");
        FrequencyId = Guard.Id(frequencyId, "Frequência");
        StartDate = startDate;
        DurationDays = Guard.OptionalPositive(durationDays, "Duração do tratamento (dias)");
        IsContinuousUse = isContinuousUse;
        AdministrationRouteId = Guard.Id(administrationRouteId, "Via de administração");
        UsageDeadline = usageDeadline;
        Notes = Guard.OptionalText(notes);
    }

    private static decimal ValidateDose(decimal dose)
    {
        if (dose <= 0)
            throw new DomainException("Campo 'Dose' deve ser maior que zero.");

        if (dose > MaxDose)
            throw new DomainException("Campo 'Dose' deve ser no máximo 9999999,999.");

        if (decimal.Round(dose, 3) != dose)
            throw new DomainException("Campo 'Dose' deve ter no máximo 3 casas decimais.");

        return dose;
    }
}
