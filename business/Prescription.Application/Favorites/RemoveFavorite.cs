using Prescription.Application.Abstractions;

namespace Prescription.Application.Favorites;

/// <summary>ECF.10.10 – removes a favorite. Not a favorite: no error, nothing changes.</summary>
public sealed class RemoveFavorite(IFavoriteMedicationRepository favorites, IUnitOfWork unitOfWork)
{
    public async Task ExecuteAsync(uint professionalId, uint medicationId, CancellationToken cancellationToken = default)
    {
        var favorite = await favorites.GetAsync(professionalId, medicationId, cancellationToken);
        if (favorite is null)
            return;

        favorites.Remove(favorite);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
