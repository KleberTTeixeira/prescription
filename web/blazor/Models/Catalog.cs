namespace Prescription.Blazor.Models;

/// <summary>Support data of the Web API (pre-populated in the database).</summary>
public sealed record Address(string Street, string Number, string? Complement, string District,
                             string City, string State, string ZipCode)
{
    /// <summary>"Rua X, 10 – apto 1, Centro, São Paulo/SP, CEP 01000-000".</summary>
    public override string ToString()
    {
        var complement = string.IsNullOrEmpty(Complement) ? "" : $" – {Complement}";
        var zipCode = ZipCode.Length == 8 ? $"{ZipCode[..5]}-{ZipCode[5..]}" : ZipCode;
        return $"{Street}, {Number}{complement}, {District}, {City}/{State}, CEP {zipCode}";
    }
}

public sealed record Client(uint Id, string FullName, DateOnly BirthDate, Sex Sex, Address Address);

public sealed record Institution(uint Id, string Name, string Cnes, Address Address, string Phone);

/// <summary>Catalog medication. DisplayName = name + concentration (stored in the prescription item).</summary>
public sealed record Medication(uint Id, string Name, string ActiveIngredient, string? Concentration,
                                string DisplayName, uint PharmaceuticalFormId, string PharmaceuticalFormName,
                                uint? ValidityPeriodDays, bool IsControlled, bool IsAntimicrobial);

/// <summary>Pharmaceutical forms, administration routes and frequencies.</summary>
public sealed record Lookup(uint Id, string Name);

public sealed record DoseUnit(uint Id, string Abbreviation, string Name);

/// <summary>Days null = no limit.</summary>
public sealed record ValidityPeriod(uint Id, string Name, uint? Days);
