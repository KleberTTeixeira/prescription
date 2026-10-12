using Prescription.Domain.Entities;

namespace Prescription.Application.Abstractions;

public interface IClientRepository
{
    Task<IReadOnlyList<Client>> ListAsync(CancellationToken cancellationToken = default);

    Task<Client?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);
}
