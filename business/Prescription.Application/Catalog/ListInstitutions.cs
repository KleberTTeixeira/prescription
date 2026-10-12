using Prescription.Application.Abstractions;

namespace Prescription.Application.Catalog;

public sealed class ListInstitutions(IInstitutionRepository institutions)
{
    public async Task<IReadOnlyList<InstitutionDto>> ExecuteAsync(CancellationToken cancellationToken = default) =>
        (await institutions.ListAsync(cancellationToken)).Select(i => i.ToDto()).ToList();
}
