using Prescription.Domain.Entities;

namespace Prescription.Application.Abstractions;

public interface IMedicationRepository
{
    /// <summary>
    /// Active medications whose name or active ingredient contains the text (all active when empty),
    /// with PharmaceuticalForm and ValidityPeriod loaded.
    /// </summary>
    Task<IReadOnlyList<Medication>> SearchActiveAsync(string? text, CancellationToken cancellationToken = default);

    /// <summary>Medication with PharmaceuticalForm and ValidityPeriod loaded.</summary>
    Task<Medication?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Medication>> GetByIdsAsync(IEnumerable<uint> ids, CancellationToken cancellationToken = default);
}
