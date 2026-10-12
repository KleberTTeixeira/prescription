using Microsoft.EntityFrameworkCore;
using Prescription.Application.Abstractions;
using Prescription.Data.Context;
using Prescription.Domain.Entities;

namespace Prescription.Data.Repositories;

internal sealed class InstitutionRepository(PrescriptionDbContext context) : IInstitutionRepository
{
    public async Task<IReadOnlyList<Institution>> ListAsync(CancellationToken cancellationToken = default) =>
        await context.Institutions.AsNoTracking().OrderBy(i => i.Name).ToListAsync(cancellationToken);

    public Task<Institution?> GetByIdAsync(uint id, CancellationToken cancellationToken = default) =>
        context.Institutions.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
}
