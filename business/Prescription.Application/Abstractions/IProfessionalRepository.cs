using Prescription.Domain.Entities;

namespace Prescription.Application.Abstractions;

public interface IProfessionalRepository
{
    Task<IReadOnlyList<Professional>> ListAsync(CancellationToken cancellationToken = default);

    Task<Professional?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);
}
