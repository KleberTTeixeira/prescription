using Prescription.Application.Abstractions;
using Prescription.Application.Catalog;
using Prescription.Application.Exceptions;
using Prescription.Domain.Entities;

namespace Prescription.Application.Favorites;

/// <summary>ECF.10.10 – marks a medication as favorite. Already favorite: no error, nothing changes.</summary>
public sealed class AddFavorite(IFavoriteMedicationRepository favorites, IProfessionalRepository professionals,
                                IMedicationRepository medications, IUnitOfWork unitOfWork, IClock clock)
{
    public async Task ExecuteAsync(uint professionalId, uint medicationId, CancellationToken cancellationToken = default)
    {
        if (await professionals.GetByIdAsync(professionalId, cancellationToken) is null)
            throw new BusinessRuleException($"Profissional {professionalId} não encontrado.");

        var medication = await medications.GetByIdAsync(medicationId, cancellationToken)
                         ?? throw new NotFoundException($"Medicamento {medicationId} não encontrado.");

        if (!medication.IsActive)
            throw new BusinessRuleException($"Medicamento '{medication.DisplayName()}' está inativo.");

        if (await favorites.GetAsync(professionalId, medicationId, cancellationToken) is not null)
            return;

        favorites.Add(new FavoriteMedication(professionalId, medicationId, clock.Now));
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
