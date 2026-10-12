using Prescription.Domain.Enums;

namespace Prescription.Application.Catalog;

public sealed record AddressDto(string Street, string Number, string? Complement, string District,
                                string City, string State, string ZipCode);

public sealed record ClientDto(uint Id, string FullName, DateOnly BirthDate, Sex Sex, AddressDto Address);

public sealed record ProfessionalDto(uint Id, string FullName, string Council, string CouncilNumber,
                                     string CouncilState);

public sealed record InstitutionDto(uint Id, string Name, string Cnes, AddressDto Address, string Phone);

/// <summary>DisplayName = name + concentration (value stored in prescription_items.medication_name).</summary>
public sealed record MedicationDto(uint Id, string Name, string ActiveIngredient, string? Concentration,
                                   string DisplayName, uint PharmaceuticalFormId, string PharmaceuticalFormName,
                                   uint? ValidityPeriodDays, bool IsControlled, bool IsAntimicrobial);

public sealed record LookupDto(uint Id, string Name);

public sealed record DoseUnitDto(uint Id, string Abbreviation, string Name);

public sealed record ValidityPeriodDto(uint Id, string Name, uint? Days);
