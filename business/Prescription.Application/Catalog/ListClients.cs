using Prescription.Application.Abstractions;

namespace Prescription.Application.Catalog;

public sealed class ListClients(IClientRepository clients)
{
    public async Task<IReadOnlyList<ClientDto>> ExecuteAsync(CancellationToken cancellationToken = default) =>
        (await clients.ListAsync(cancellationToken)).Select(c => c.ToDto()).ToList();
}
