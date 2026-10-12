using Prescription.Domain.Entities;
using Prescription.Domain.ValueObjects;

namespace Prescription.Application.Catalog;

internal static class CatalogMapper
{
    /// <summary>Name + concentration, e.g. "Amoxicilina 500 mg".</summary>
    public static string DisplayName(this Medication medication) =>
        string.IsNullOrWhiteSpace(medication.Concentration)
            ? medication.Name
            : $"{medication.Name} {medication.Concentration}";

    public static AddressDto ToDto(this Address address) =>
        new(address.Street, address.Number, address.Complement, address.District,
            address.City, address.State, address.ZipCode);

    public static ClientDto ToDto(this Client client) =>
        new(client.Id, client.FullName, client.BirthDate, client.Sex, client.Address.ToDto());

    public static ProfessionalDto ToDto(this Professional professional) =>
        new(professional.Id, professional.FullName, professional.Council,
            professional.CouncilNumber, professional.CouncilState);

    public static InstitutionDto ToDto(this Institution institution) =>
        new(institution.Id, institution.Name, institution.Cnes, institution.Address.ToDto(), institution.Phone);

    public static MedicationDto ToDto(this Medication medication) =>
        new(medication.Id, medication.Name, medication.ActiveIngredient, medication.Concentration,
            medication.DisplayName(), medication.PharmaceuticalFormId, medication.PharmaceuticalForm.Name,
            medication.ValidityPeriod.Days, medication.IsControlled, medication.IsAntimicrobial);

    public static LookupDto ToDto(this PharmaceuticalForm form) => new(form.Id, form.Name);

    public static LookupDto ToDto(this AdministrationRoute route) => new(route.Id, route.Name);

    public static LookupDto ToDto(this Frequency frequency) => new(frequency.Id, frequency.Name);

    public static DoseUnitDto ToDto(this DoseUnit unit) => new(unit.Id, unit.Abbreviation, unit.Name);

    public static ValidityPeriodDto ToDto(this ValidityPeriod period) => new(period.Id, period.Name, period.Days);
}
