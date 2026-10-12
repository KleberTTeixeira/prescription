using Prescription.Domain.Exceptions;
using Prescription.Domain.ValueObjects;

namespace Prescription.Domain.Entities;

/// <summary>Table institutions – place of care (ECF.10.04).</summary>
public sealed class Institution
{
    public uint Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Cnes { get; private set; } = string.Empty;
    public Address Address { get; private set; } = null!;
    public string Phone { get; private set; } = string.Empty;

    private Institution() { }

    public Institution(string name, string cnes, Address address, string phone)
    {
        Name = Guard.Required(name, 150, "Nome da instituição");
        Cnes = Guard.Digits(cnes, 7, "CNES");
        Address = address ?? throw new DomainException("Campo 'Endereço da instituição' é obrigatório.");
        Phone = Guard.Required(phone, 20, "Telefone da instituição");
    }
}
