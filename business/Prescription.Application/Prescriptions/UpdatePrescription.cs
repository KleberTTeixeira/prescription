using Prescription.Application.Abstractions;

namespace Prescription.Application.Prescriptions;

/// <summary>Changes a not issued prescription of the professional (the format can change too).</summary>
public sealed class UpdatePrescription(IPrescriptionRepository prescriptions, PrescriptionInputResolver resolver,
                                       IUnitOfWork unitOfWork, IClock clock)
{
    public async Task<PrescriptionDto> ExecuteAsync(uint professionalId, uint id, PrescriptionRequest request,
                                                    CancellationToken cancellationToken = default)
    {
        var prescription = await prescriptions.GetByIdAsync(id, cancellationToken)
                           ?? throw PrescriptionOwnership.NotFound(id);
        PrescriptionOwnership.EnsureOwner(prescription, professionalId);

        var items = await resolver.ResolveAsync(request, cancellationToken);

        prescription.Update(request.ClientId, prescription.ProfessionalId, request.InstitutionId, request.Format,
                            request.Category, request.FreeText, items, clock.Now);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var saved = await prescriptions.GetByIdAsync(id, cancellationToken) ?? throw PrescriptionOwnership.NotFound(id);
        return saved.ToDto();
    }
}
