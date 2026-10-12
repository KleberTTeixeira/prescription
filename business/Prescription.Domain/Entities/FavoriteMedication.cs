using Prescription.Domain.Exceptions;

namespace Prescription.Domain.Entities;

/// <summary>Table favorite_medications – favorites per professional (ECF.10.10).</summary>
public sealed class FavoriteMedication
{
    public uint ProfessionalId { get; private set; }
    public uint MedicationId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Professional Professional { get; private set; } = null!;
    public Medication Medication { get; private set; } = null!;

    private FavoriteMedication() { }

    public FavoriteMedication(uint professionalId, uint medicationId, DateTime now)
    {
        ProfessionalId = Guard.Id(professionalId, "Profissional");
        MedicationId = Guard.Id(medicationId, "Medicamento");
        CreatedAt = now;
    }
}
