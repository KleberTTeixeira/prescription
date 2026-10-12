using Prescription.Domain.Exceptions;

namespace Prescription.Domain.Entities;

/// <summary>Table administration_routes (e.g. oral, intravenous).</summary>
public sealed class AdministrationRoute
{
    public uint Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private AdministrationRoute() { }

    public AdministrationRoute(string name)
    {
        Name = Guard.Required(name, 60, "Via de administração");
    }
}
