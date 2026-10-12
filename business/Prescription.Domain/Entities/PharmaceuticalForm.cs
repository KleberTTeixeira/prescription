using Prescription.Domain.Exceptions;

namespace Prescription.Domain.Entities;

/// <summary>Table pharmaceutical_forms (e.g. tablet, ampoule).</summary>
public sealed class PharmaceuticalForm
{
    public uint Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private PharmaceuticalForm() { }

    public PharmaceuticalForm(string name)
    {
        Name = Guard.Required(name, 60, "Forma farmacêutica");
    }
}
