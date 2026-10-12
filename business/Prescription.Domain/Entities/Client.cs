using Prescription.Domain.Enums;
using Prescription.Domain.Exceptions;
using Prescription.Domain.ValueObjects;

namespace Prescription.Domain.Entities;

/// <summary>Table clients (ECF.10.04).</summary>
public sealed class Client
{
    public uint Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public DateOnly BirthDate { get; private set; }
    public Sex Sex { get; private set; }
    public Address Address { get; private set; } = null!;

    private Client() { }

    public Client(string fullName, DateOnly birthDate, Sex sex, Address address)
    {
        FullName = Guard.Required(fullName, 150, "Nome do cliente");
        BirthDate = birthDate;
        Sex = Guard.Defined(sex, "Sexo");
        Address = address ?? throw new DomainException("Campo 'Endereço do cliente' é obrigatório.");
    }
}
