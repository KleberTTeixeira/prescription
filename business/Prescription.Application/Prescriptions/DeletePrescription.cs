using Prescription.Application.Abstractions;

namespace Prescription.Application.Prescriptions;

/// <summary>Deletes a prescription of the professional (allowed even when issued).</summary>
public sealed class DeletePrescription(IPrescriptionRepository prescriptions, IUnitOfWork unitOfWork)
{
    public async Task ExecuteAsync(uint professionalId, uint id, CancellationToken cancellationToken = default)
    {
        var prescription = await prescriptions.GetByIdAsync(id, cancellationToken)
                           ?? throw PrescriptionOwnership.NotFound(id);
        PrescriptionOwnership.EnsureOwner(prescription, professionalId);

        prescriptions.Remove(prescription);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
