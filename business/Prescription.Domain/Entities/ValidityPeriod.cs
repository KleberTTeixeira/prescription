using Prescription.Domain.Exceptions;

namespace Prescription.Domain.Entities;

/// <summary>
/// Table validity_periods (ECF.10.03 "prazo de utilização"). Days null = no validity limit.
/// </summary>
public sealed class ValidityPeriod
{
    public uint Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public uint? Days { get; private set; }

    private ValidityPeriod() { }

    public ValidityPeriod(string name, uint? days)
    {
        Name = Guard.Required(name, 80, "Prazo de utilização");
        Days = Guard.OptionalPositive(days, "Dias do prazo de utilização");
    }
}
