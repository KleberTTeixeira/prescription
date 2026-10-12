using Prescription.Domain.Exceptions;

namespace Prescription.Domain.Entities;

/// <summary>Table frequencies (e.g. every 8 hours).</summary>
public sealed class Frequency
{
    public uint Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private Frequency() { }

    public Frequency(string name)
    {
        Name = Guard.Required(name, 60, "Frequência");
    }
}
