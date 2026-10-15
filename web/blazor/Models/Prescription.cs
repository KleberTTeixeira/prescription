namespace Prescription.Blazor.Models;

/// <summary>ECF.10.01 Unstructured (free text) / ECF.10.03 Structured (items).</summary>
public enum PrescriptionFormat
{
    Unstructured = 1,
    Structured = 2
}

public enum PrescriptionCategory
{
    Common = 1,
    SpecialControl = 2,
    Antimicrobial = 3
}

public enum Sex
{
    Male = 1,
    Female = 2
}

/// <summary>Screen texts (Portuguese) of the enums.</summary>
public static class Labels
{
    public static string ToLabel(this PrescriptionFormat format) => format switch
    {
        PrescriptionFormat.Unstructured => "Não estruturada",
        PrescriptionFormat.Structured => "Estruturada",
        _ => format.ToString()
    };

    public static string ToLabel(this PrescriptionCategory category) => category switch
    {
        PrescriptionCategory.Common => "Comum",
        PrescriptionCategory.SpecialControl => "Controle especial",
        PrescriptionCategory.Antimicrobial => "Antimicrobiano",
        _ => category.ToString()
    };

    public static string ToLabel(this Sex sex) => sex switch
    {
        Sex.Male => "Masculino",
        Sex.Female => "Feminino",
        _ => sex.ToString()
    };
}

/// <summary>DATE columns as DateOnly; DATETIME as DateTime in Brasília time.</summary>
public sealed record PrescriptionItem(uint Id, uint? MedicationId, string MedicationName, uint PharmaceuticalFormId,
                                      string PharmaceuticalFormName, decimal Dose, uint DoseUnitId,
                                      string DoseUnitAbbreviation, uint FrequencyId, string FrequencyName,
                                      DateOnly StartDate, uint? DurationDays, bool IsContinuousUse,
                                      uint AdministrationRouteId, string AdministrationRouteName,
                                      DateOnly? UsageDeadline, string? Notes);

/// <summary>Named MedicalPrescription (as in the Domain) to avoid a conflict with the Prescription namespace. IsIssued: printed for the first time (confirmed); locked, only view, reprint and delete.</summary>
public sealed record MedicalPrescription(uint Id, uint ClientId, string ClientName, uint ProfessionalId,
                                  string ProfessionalName, uint InstitutionId, string InstitutionName,
                                  PrescriptionFormat Format, PrescriptionCategory Category, string? FreeText,
                                  DateTime IssuedAt, DateTime? PrintedAt, bool IsIssued,
                                  IReadOnlyList<PrescriptionItem> Items);

/// <summary>Row of the prescription list.</summary>
public sealed record PrescriptionListItem(uint Id, string ClientName, string InstitutionName,
                                          PrescriptionFormat Format, PrescriptionCategory Category,
                                          DateTime IssuedAt, bool IsIssued, int ItemCount);

public sealed record PrescriptionPage(IReadOnlyList<PrescriptionListItem> Rows, int Total);

/// <summary>Columns of the prescription list that can be sorted.</summary>
public enum PrescriptionSortField
{
    Id,
    IssuedAt,
    ClientName,
    Format,
    Category,
    ItemCount,
    IsIssued
}

public enum PrescriptionSortDirection
{
    Asc,
    Desc
}

public sealed record PrescriptionSort(PrescriptionSortField Field, PrescriptionSortDirection Direction);

public sealed record PrescriptionListQuery(string ClientName, PrescriptionSort Sort, int Skip, int Top);

/// <summary>Catalog medication: MedicationId filled (name and form come from the catalog); otherwise name and form typed.</summary>
public sealed record PrescriptionItemRequest(uint? MedicationId, string? MedicationName, uint? PharmaceuticalFormId,
                                             decimal Dose, uint DoseUnitId, uint FrequencyId, DateOnly StartDate,
                                             uint? DurationDays, bool IsContinuousUse, uint AdministrationRouteId,
                                             DateOnly? UsageDeadline, string? Notes);

public sealed record PrescriptionRequest(uint ClientId, uint InstitutionId, PrescriptionFormat Format,
                                         PrescriptionCategory Category, string? FreeText,
                                         IReadOnlyList<PrescriptionItemRequest> Items);

/// <summary>ECF.10.04 – address only for special control; age and sex only for antimicrobial.</summary>
public sealed record PrintClient(string FullName, Address? Address, int? Age, Sex? Sex);

public sealed record PrescriptionPrintData(uint Id, PrescriptionFormat Format, PrescriptionCategory Category,
                                           string? FreeText, DateTime IssuedAt, DateTime? PrintedAt,
                                           PrintClient Client, Professional Professional, Institution Institution,
                                           IReadOnlyList<PrescriptionItem> Items);
