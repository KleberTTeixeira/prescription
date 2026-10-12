using Prescription.Application.Abstractions;
using Prescription.Domain.Entities;

namespace Prescription.Application.Prescriptions;

/// <summary>Creates a prescription (ECF.10.01 / ECF.10.03). IssuedAt = current date and time.</summary>
public sealed class CreatePrescription(IPrescriptionRepository prescriptions, PrescriptionInputResolver resolver,
                                       IUnitOfWork unitOfWork, IClock clock)
{
    public async Task<PrescriptionDto> ExecuteAsync(uint professionalId, PrescriptionRequest request,
                                                    CancellationToken cancellationToken = default)
    {
        await resolver.EnsureProfessionalExistsAsync(professionalId, cancellationToken);
        var items = await resolver.ResolveAsync(request, cancellationToken);

        var prescription = MedicalPrescription.Create(request.ClientId, professionalId, request.InstitutionId,
                                                      request.Format, request.Category, request.FreeText,
                                                      items, clock.Now);

        prescriptions.Add(prescription);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var saved = await prescriptions.GetByIdAsync(prescription.Id, cancellationToken)
                    ?? throw PrescriptionOwnership.NotFound(prescription.Id);
        return saved.ToDto();
    }
}
