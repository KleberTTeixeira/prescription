using Prescription.Domain.Entities;

namespace Prescription.Application.Abstractions;

public interface IPrescriptionRepository
{
    /// <summary>Not executed query, used for listing with OData.</summary>
    IQueryable<MedicalPrescription> Query();

    /// <summary>Prescription with items and all navigations loaded.</summary>
    Task<MedicalPrescription?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);

    void Add(MedicalPrescription prescription);

    void Remove(MedicalPrescription prescription);
}
