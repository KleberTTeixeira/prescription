using Prescription.Domain.Entities;

namespace Prescription.Application.Abstractions;

public interface IInstitutionRepository
{
    Task<IReadOnlyList<Institution>> ListAsync(CancellationToken cancellationToken = default);

    Task<Institution?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);
}
