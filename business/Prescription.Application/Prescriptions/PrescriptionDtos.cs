using Prescription.Application.Catalog;
using Prescription.Domain.Enums;

namespace Prescription.Application.Prescriptions;

/// <summary>Create/update input. The professional comes from the X-Professional-Id header.</summary>
public sealed record PrescriptionRequest(uint ClientId, uint InstitutionId, PrescriptionFormat Format,
                                         PrescriptionCategory Category, string? FreeText,
                                         IReadOnlyList<PrescriptionItemRequest>? Items);

/// <summary>
/// For a catalog medication (MedicationId filled) the name and the pharmaceutical form come from the catalog;
/// MedicationName and PharmaceuticalFormId are used only for medications not in the catalog.
/// </summary>
public sealed record PrescriptionItemRequest(uint? MedicationId, string? MedicationName, uint? PharmaceuticalFormId,
                                             decimal Dose, uint DoseUnitId, uint FrequencyId, DateOnly StartDate,
                                             uint? DurationDays, bool IsContinuousUse, uint AdministrationRouteId,
                                             DateOnly? UsageDeadline, string? Notes);

public sealed record PrescriptionItemDto(uint Id, uint? MedicationId, string MedicationName,
                                         uint PharmaceuticalFormId, string PharmaceuticalFormName, decimal Dose,
                                         uint DoseUnitId, string DoseUnitAbbreviation, uint FrequencyId,
                                         string FrequencyName, DateOnly StartDate, uint? DurationDays,
                                         bool IsContinuousUse, uint AdministrationRouteId,
                                         string AdministrationRouteName, DateOnly? UsageDeadline, string? Notes);

public sealed record PrescriptionDto(uint Id, uint ClientId, string ClientName, uint ProfessionalId,
                                     string ProfessionalName, uint InstitutionId, string InstitutionName,
                                     PrescriptionFormat Format, PrescriptionCategory Category, string? FreeText,
                                     DateTime IssuedAt, DateTime? PrintedAt, bool IsIssued,
                                     IReadOnlyList<PrescriptionItemDto> Items);

/// <summary>Row of the prescription list (queried with OData; class with init properties so EF can project it).</summary>
public sealed class PrescriptionListItemDto
{
    public uint Id { get; init; }
    public uint ClientId { get; init; }
    public string ClientName { get; init; } = string.Empty;
    public uint ProfessionalId { get; init; }
    public string ProfessionalName { get; init; } = string.Empty;
    public uint InstitutionId { get; init; }
    public string InstitutionName { get; init; } = string.Empty;
    public PrescriptionFormat Format { get; init; }
    public PrescriptionCategory Category { get; init; }
    public DateTime IssuedAt { get; init; }
    public DateTime? PrintedAt { get; init; }
    public bool IsIssued { get; init; }
    public int ItemCount { get; init; }
}

/// <summary>
/// ECF.10.04 print data. Client address only for special control; client age and sex only for antimicrobial.
/// </summary>
public sealed record PrescriptionPrintDto(uint Id, PrescriptionFormat Format, PrescriptionCategory Category,
                                          string? FreeText, DateTime IssuedAt, DateTime? PrintedAt,
                                          PrintClientDto Client, ProfessionalDto Professional,
                                          InstitutionDto Institution, IReadOnlyList<PrescriptionItemDto> Items);

public sealed record PrintClientDto(string FullName, AddressDto? Address, int? Age, Sex? Sex);
