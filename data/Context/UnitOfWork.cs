using Prescription.Application.Abstractions;

namespace Prescription.Data.Context;

internal sealed class UnitOfWork(PrescriptionDbContext context) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
