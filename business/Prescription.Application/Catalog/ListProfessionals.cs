using Prescription.Application.Abstractions;

namespace Prescription.Application.Catalog;

/// <summary>Used by the initial professional selection screen (instead of login).</summary>
public sealed class ListProfessionals(IProfessionalRepository professionals)
{
    public async Task<IReadOnlyList<ProfessionalDto>> ExecuteAsync(CancellationToken cancellationToken = default) =>
        (await professionals.ListAsync(cancellationToken)).Select(p => p.ToDto()).ToList();
}
