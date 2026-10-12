using Prescription.Domain.Exceptions;

namespace Prescription.Domain.Entities;

/// <summary>Table medications – catalog (ECF.10.03 b).</summary>
public sealed class Medication
{
    public uint Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string ActiveIngredient { get; private set; } = string.Empty;
    public string? Concentration { get; private set; }
    public uint PharmaceuticalFormId { get; private set; }
    public uint ValidityPeriodId { get; private set; }
    public bool IsControlled { get; private set; }
    public bool IsAntimicrobial { get; private set; }
    public bool IsActive { get; private set; }

    public PharmaceuticalForm PharmaceuticalForm { get; private set; } = null!;
    public ValidityPeriod ValidityPeriod { get; private set; } = null!;

    private Medication() { }

    public Medication(string name, string activeIngredient, string? concentration,
                      uint pharmaceuticalFormId, uint validityPeriodId,
                      bool isControlled, bool isAntimicrobial, bool isActive)
    {
        Name = Guard.Required(name, 150, "Nome do medicamento");
        ActiveIngredient = Guard.Required(activeIngredient, 150, "Princípio ativo");
        Concentration = Guard.Optional(concentration, 60, "Concentração");
        PharmaceuticalFormId = Guard.Id(pharmaceuticalFormId, "Forma farmacêutica");
        ValidityPeriodId = Guard.Id(validityPeriodId, "Prazo de utilização");
        IsControlled = isControlled;
        IsAntimicrobial = isAntimicrobial;
        IsActive = isActive;
    }
}
