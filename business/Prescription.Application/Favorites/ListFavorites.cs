using Prescription.Application.Abstractions;
using Prescription.Application.Catalog;

namespace Prescription.Application.Favorites;

/// <summary>ECF.10.10 – favorite medications of the logged professional (active medications only).</summary>
public sealed class ListFavorites(IFavoriteMedicationRepository favorites)
{
    public async Task<IReadOnlyList<MedicationDto>> ExecuteAsync(uint professionalId,
                                                                 CancellationToken cancellationToken = default) =>
        (await favorites.ListActiveByProfessionalAsync(professionalId, cancellationToken))
            .Select(f => f.Medication.ToDto())
            .ToList();
}
