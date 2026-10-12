using Microsoft.EntityFrameworkCore;
using Prescription.Application.Abstractions;
using Prescription.Data.Context;
using Prescription.Domain.Entities;

namespace Prescription.Data.Repositories;

internal sealed class PrescriptionTemplateRepository(PrescriptionDbContext context) : IPrescriptionTemplateRepository
{
    public Task<PrescriptionTemplate?> GetByIdAsync(uint id, CancellationToken cancellationToken = default) =>
        context.PrescriptionTemplates.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PrescriptionTemplate>> ListByProfessionalAsync(
        uint professionalId, CancellationToken cancellationToken = default) =>
        await context.PrescriptionTemplates.AsNoTracking()
                     .Where(t => t.ProfessionalId == professionalId)
                     .OrderBy(t => t.Title)
                     .ToListAsync(cancellationToken);

    /// <summary>Comparison follows the database collation (utf8mb4_0900_ai_ci: case and accent insensitive).</summary>
    public Task<bool> TitleExistsAsync(uint professionalId, string title, uint? exceptTemplateId,
                                       CancellationToken cancellationToken = default) =>
        context.PrescriptionTemplates.AnyAsync(t => t.ProfessionalId == professionalId
                                                    && t.Title == title
                                                    && (exceptTemplateId == null || t.Id != exceptTemplateId),
                                               cancellationToken);

    public void Add(PrescriptionTemplate template) => context.PrescriptionTemplates.Add(template);

    public void Remove(PrescriptionTemplate template) => context.PrescriptionTemplates.Remove(template);
}
