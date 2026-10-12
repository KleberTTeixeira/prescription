using Prescription.Domain.Exceptions;

namespace Prescription.Domain.Entities;

/// <summary>Table dose_units (e.g. mg, mL).</summary>
public sealed class DoseUnit
{
    public uint Id { get; private set; }
    public string Abbreviation { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;

    private DoseUnit() { }

    public DoseUnit(string abbreviation, string name)
    {
        Abbreviation = Guard.Required(abbreviation, 20, "Abreviação da unidade");
        Name = Guard.Required(name, 60, "Unidade de dose");
    }
}
