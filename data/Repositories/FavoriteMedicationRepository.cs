using Microsoft.EntityFrameworkCore;
using Prescription.Application.Abstractions;
using Prescription.Data.Context;
using Prescription.Domain.Entities;

namespace Prescription.Data.Repositories;

internal sealed class FavoriteMedicationRepository(PrescriptionDbContext context) : IFavoriteMedicationRepository
{
    public Task<FavoriteMedication?> GetAsync(uint professionalId, uint medicationId,
                                              CancellationToken cancellationToken = default) =>
        context.FavoriteMedications.FirstOrDefaultAsync(
            f => f.ProfessionalId == professionalId && f.MedicationId == medicationId, cancellationToken);

    public async Task<IReadOnlyList<FavoriteMedication>> ListActiveByProfessionalAsync(
        uint professionalId, CancellationToken cancellationToken = default) =>
        await context.FavoriteMedications.AsNoTracking()
                     .Include(f => f.Medication).ThenInclude(m => m.PharmaceuticalForm)
                     .Include(f => f.Medication).ThenInclude(m => m.ValidityPeriod)
                     .Where(f => f.ProfessionalId == professionalId && f.Medication.IsActive)
                     .OrderBy(f => f.Medication.Name).ThenBy(f => f.Medication.Concentration)
                     .ToListAsync(cancellationToken);

    public void Add(FavoriteMedication favorite) => context.FavoriteMedications.Add(favorite);

    public void Remove(FavoriteMedication favorite) => context.FavoriteMedications.Remove(favorite);
}
