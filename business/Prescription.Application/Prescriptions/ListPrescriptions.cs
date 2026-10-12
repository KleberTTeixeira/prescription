using Prescription.Application.Abstractions;

namespace Prescription.Application.Prescriptions;

/// <summary>
/// Prescriptions of the logged professional as a not executed query; the API applies OData
/// ($filter, $orderby, $select...) and EF Core translates everything into a single SQL.
/// </summary>
public sealed class ListPrescriptions(IPrescriptionRepository prescriptions)
{
    public IQueryable<PrescriptionListItemDto> Execute(uint professionalId) =>
        prescriptions.Query()
            .Where(p => p.ProfessionalId == professionalId)
            .Select(p => new PrescriptionListItemDto
            {
                Id = p.Id,
                ClientId = p.ClientId,
                ClientName = p.Client.FullName,
                ProfessionalId = p.ProfessionalId,
                ProfessionalName = p.Professional.FullName,
                InstitutionId = p.InstitutionId,
                InstitutionName = p.Institution.Name,
                Format = p.Format,
                Category = p.Category,
                IssuedAt = p.IssuedAt,
                PrintedAt = p.PrintedAt,
                IsIssued = p.PrintedAt != null,
                ItemCount = p.Items.Count
            });
}
