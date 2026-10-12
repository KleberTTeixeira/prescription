using Prescription.Domain.Entities;

namespace Prescription.Application.Abstractions;

public interface IPrescriptionTemplateRepository
{
    Task<PrescriptionTemplate?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PrescriptionTemplate>> ListByProfessionalAsync(uint professionalId,
                                                                     CancellationToken cancellationToken = default);

    Task<bool> TitleExistsAsync(uint professionalId, string title, uint? exceptTemplateId,
                                CancellationToken cancellationToken = default);

    void Add(PrescriptionTemplate template);

    void Remove(PrescriptionTemplate template);
}
