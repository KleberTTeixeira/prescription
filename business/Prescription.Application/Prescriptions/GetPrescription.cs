using Prescription.Application.Abstractions;

namespace Prescription.Application.Prescriptions;

/// <summary>Views a prescription (any professional can view).</summary>
public sealed class GetPrescription(IPrescriptionRepository prescriptions)
{
    public async Task<PrescriptionDto> ExecuteAsync(uint id, CancellationToken cancellationToken = default)
    {
        var prescription = await prescriptions.GetByIdAsync(id, cancellationToken)
                           ?? throw PrescriptionOwnership.NotFound(id);
        return prescription.ToDto();
    }
}
