using Prescription.Domain.Entities;

namespace Prescription.Application.Prescriptions;

internal static class PrescriptionMapper
{
    public static PrescriptionDto ToDto(this MedicalPrescription p) =>
        new(p.Id, p.ClientId, p.Client.FullName, p.ProfessionalId, p.Professional.FullName,
            p.InstitutionId, p.Institution.Name, p.Format, p.Category, p.FreeText,
            p.IssuedAt, p.PrintedAt, p.IsIssued, p.Items.ToDtos());

    public static IReadOnlyList<PrescriptionItemDto> ToDtos(this IEnumerable<PrescriptionItem> items) =>
        items.Select(i => new PrescriptionItemDto(
            i.Id, i.MedicationId, i.MedicationName, i.PharmaceuticalFormId, i.PharmaceuticalForm.Name, i.Dose,
            i.DoseUnitId, i.DoseUnit.Abbreviation, i.FrequencyId, i.Frequency.Name, i.StartDate, i.DurationDays,
            i.IsContinuousUse, i.AdministrationRouteId, i.AdministrationRoute.Name, i.UsageDeadline, i.Notes))
        .ToList();
}
