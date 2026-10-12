using Prescription.Domain.Exceptions;

namespace Prescription.Domain.Entities;

/// <summary>Table professionals – prescriber (ECF.10.04).</summary>
public sealed class Professional
{
    public uint Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Council { get; private set; } = string.Empty;
    public string CouncilNumber { get; private set; } = string.Empty;
    public string CouncilState { get; private set; } = string.Empty;

    private Professional() { }

    public Professional(string fullName, string council, string councilNumber, string councilState)
    {
        FullName = Guard.Required(fullName, 150, "Nome do profissional");
        Council = Guard.Required(council, 10, "Conselho");
        CouncilNumber = Guard.Required(councilNumber, 20, "Número do conselho");
        CouncilState = Guard.Required(councilState, 2, "UF do conselho");
    }
}
