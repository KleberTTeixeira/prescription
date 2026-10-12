using Prescription.Application.Abstractions;

namespace Prescription.Application.Catalog;

public sealed class ListFrequencies(ILookupRepository lookups)
{
    public async Task<IReadOnlyList<LookupDto>> ExecuteAsync(CancellationToken cancellationToken = default) =>
        (await lookups.ListFrequenciesAsync(cancellationToken)).Select(x => x.ToDto()).ToList();
}
