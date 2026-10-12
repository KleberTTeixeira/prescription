using Microsoft.EntityFrameworkCore;
using Prescription.Application.Abstractions;
using Prescription.Data.Context;
using Prescription.Domain.Entities;

namespace Prescription.Data.Repositories;

internal sealed class ProfessionalRepository(PrescriptionDbContext context) : IProfessionalRepository
{
    public async Task<IReadOnlyList<Professional>> ListAsync(CancellationToken cancellationToken = default) =>
        await context.Professionals.AsNoTracking().OrderBy(p => p.FullName).ToListAsync(cancellationToken);

    public Task<Professional?> GetByIdAsync(uint id, CancellationToken cancellationToken = default) =>
        context.Professionals.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
}
