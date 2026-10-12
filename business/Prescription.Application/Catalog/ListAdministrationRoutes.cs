using Prescription.Application.Abstractions;

namespace Prescription.Application.Catalog;

public sealed class ListAdministrationRoutes(ILookupRepository lookups)
{
    public async Task<IReadOnlyList<LookupDto>> ExecuteAsync(CancellationToken cancellationToken = default) =>
        (await lookups.ListAdministrationRoutesAsync(cancellationToken)).Select(x => x.ToDto()).ToList();
}
