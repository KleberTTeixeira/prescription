using Prescription.Application.Abstractions;

namespace Prescription.Application.Prescriptions;

/// <summary>
/// Called after the user confirms the print: issues the prescription (PrintedAt, only the first time).
/// From then on the prescription is locked.
/// </summary>
public sealed class MarkPrescriptionAsPrinted(IPrescriptionRepository prescriptions, IUnitOfWork unitOfWork,
                                              IClock clock)
{
    public async Task<PrescriptionDto> ExecuteAsync(uint professionalId, uint id,
                                                    CancellationToken cancellationToken = default)
    {
        var prescription = await prescriptions.GetByIdAsync(id, cancellationToken)
                           ?? throw PrescriptionOwnership.NotFound(id);
        PrescriptionOwnership.EnsureOwner(prescription, professionalId);

        prescription.MarkAsPrinted(clock.Now);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return prescription.ToDto();
    }
}
