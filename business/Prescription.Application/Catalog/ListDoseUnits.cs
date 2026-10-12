using Prescription.Application.Abstractions;

namespace Prescription.Application.Catalog;

public sealed class ListDoseUnits(ILookupRepository lookups)
{
    public async Task<IReadOnlyList<DoseUnitDto>> ExecuteAsync(CancellationToken cancellationToken = default) =>
        (await lookups.ListDoseUnitsAsync(cancellationToken)).Select(x => x.ToDto()).ToList();
}
