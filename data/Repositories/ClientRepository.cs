using Microsoft.EntityFrameworkCore;
using Prescription.Application.Abstractions;
using Prescription.Data.Context;
using Prescription.Domain.Entities;

namespace Prescription.Data.Repositories;

internal sealed class ClientRepository(PrescriptionDbContext context) : IClientRepository
{
    public async Task<IReadOnlyList<Client>> ListAsync(CancellationToken cancellationToken = default) =>
        await context.Clients.AsNoTracking().OrderBy(c => c.FullName).ToListAsync(cancellationToken);

    public Task<Client?> GetByIdAsync(uint id, CancellationToken cancellationToken = default) =>
        context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
}
