using Prescription.Domain.Exceptions;

namespace Prescription.Domain.ValueObjects;

/// <summary>
/// Address shared by clients and institutions (columns address_*). Compared by value.
/// </summary>
public sealed record Address
{
    public string Street { get; private set; } = string.Empty;
    public string Number { get; private set; } = string.Empty;
    public string? Complement { get; private set; }
    public string District { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string ZipCode { get; private set; } = string.Empty;

    private Address() { }

    public Address(string street, string number, string? complement,
                   string district, string city, string state, string zipCode)
    {
        Street = Guard.Required(street, 150, "Logradouro");
        Number = Guard.Required(number, 10, "Número");
        Complement = Guard.Optional(complement, 60, "Complemento");
        District = Guard.Required(district, 80, "Bairro");
        City = Guard.Required(city, 80, "Cidade");
        State = Guard.Required(state, 2, "UF");
        ZipCode = Guard.Digits(zipCode, 8, "CEP");
    }
}
