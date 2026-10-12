namespace Prescription.Application.Abstractions;

/// <summary>Saves all pending changes of the repositories in a single transaction.</summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
