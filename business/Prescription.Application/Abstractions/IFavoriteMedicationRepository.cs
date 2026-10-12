using Prescription.Domain.Entities;

namespace Prescription.Application.Abstractions;

public interface IFavoriteMedicationRepository
{
    Task<FavoriteMedication?> GetAsync(uint professionalId, uint medicationId,
                                       CancellationToken cancellationToken = default);

    /// <summary>Favorites with active medications only, with Medication, PharmaceuticalForm and ValidityPeriod loaded.</summary>
    Task<IReadOnlyList<FavoriteMedication>> ListActiveByProfessionalAsync(uint professionalId,
                                                                         CancellationToken cancellationToken = default);

    void Add(FavoriteMedication favorite);

    void Remove(FavoriteMedication favorite);
}
