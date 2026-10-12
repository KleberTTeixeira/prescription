using Prescription.Application.Abstractions;

namespace Prescription.Application.Catalog;

public sealed class ListValidityPeriods(ILookupRepository lookups)
{
    public async Task<IReadOnlyList<ValidityPeriodDto>> ExecuteAsync(CancellationToken cancellationToken = default) =>
        (await lookups.ListValidityPeriodsAsync(cancellationToken)).Select(x => x.ToDto()).ToList();
}
