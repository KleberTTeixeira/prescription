using Prescription.Application.Exceptions;
using Prescription.Domain.Entities;

namespace Prescription.Application.Prescriptions;

internal static class PrescriptionOwnership
{
    /// <summary>Only the owner professional can change, issue or delete the prescription.</summary>
    public static void EnsureOwner(MedicalPrescription prescription, uint professionalId)
    {
        if (prescription.ProfessionalId != professionalId)
            throw new BusinessRuleException("Receita pertence a outro profissional.");
    }

    public static NotFoundException NotFound(uint id) => new($"Receita {id} não encontrada.");
}
