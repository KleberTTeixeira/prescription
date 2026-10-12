using Microsoft.EntityFrameworkCore;
using Prescription.Application.Abstractions;
using Prescription.Data.Context;
using Prescription.Domain.Entities;

namespace Prescription.Data.Repositories;

internal sealed class MedicationRepository(PrescriptionDbContext context) : IMedicationRepository
{
    /// <summary>
    /// Case and accent insensitive search (LIKE follows the utf8mb4_0900_ai_ci collation).
    /// Contains is not used: the provider translates it to LOCATE, which is accent sensitive.
    /// </summary>
    public async Task<IReadOnlyList<Medication>> SearchActiveAsync(string? text,
                                                                   CancellationToken cancellationToken = default)
    {
        var query = WithDetails().Where(m => m.IsActive);

        if (!string.IsNullOrWhiteSpace(text))
        {
            var pattern = $"%{EscapeLike(text.Trim())}%";
            query = query.Where(m => EF.Functions.Like(m.Name, pattern)
                                     || EF.Functions.Like(m.ActiveIngredient, pattern));
        }

        return await query.OrderBy(m => m.Name).ThenBy(m => m.Concentration).ToListAsync(cancellationToken);
    }

    public Task<Medication?> GetByIdAsync(uint id, CancellationToken cancellationToken = default) =>
        WithDetails().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Medication>> GetByIdsAsync(IEnumerable<uint> ids,
                                                               CancellationToken cancellationToken = default)
    {
        var list = ids.Distinct().ToList();
        return await WithDetails().Where(m => list.Contains(m.Id)).ToListAsync(cancellationToken);
    }

    /// <summary>% and _ typed by the user are plain text (MySQL LIKE escape character is \).</summary>
    private static string EscapeLike(string text) =>
        text.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");

    private IQueryable<Medication> WithDetails() =>
        context.Medications.AsNoTracking()
               .Include(m => m.PharmaceuticalForm)
               .Include(m => m.ValidityPeriod);
}
