using Prescription.Application.Abstractions;

namespace Prescription.Application.Catalog;

public sealed class ListPharmaceuticalForms(ILookupRepository lookups)
{
    public async Task<IReadOnlyList<LookupDto>> ExecuteAsync(CancellationToken cancellationToken = default) =>
        (await lookups.ListPharmaceuticalFormsAsync(cancellationToken)).Select(x => x.ToDto()).ToList();
}
