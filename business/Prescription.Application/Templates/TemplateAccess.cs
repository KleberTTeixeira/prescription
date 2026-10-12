using Prescription.Application.Abstractions;
using Prescription.Application.Exceptions;
using Prescription.Domain.Entities;

namespace Prescription.Application.Templates;

internal static class TemplateAccess
{
    /// <summary>Loads a template of the professional (404 if missing, 400 if it belongs to another professional).</summary>
    public static async Task<PrescriptionTemplate> GetOwnAsync(this IPrescriptionTemplateRepository templates,
                                                               uint professionalId, uint id,
                                                               CancellationToken cancellationToken)
    {
        var template = await templates.GetByIdAsync(id, cancellationToken)
                       ?? throw new NotFoundException($"Texto padrão {id} não encontrado.");

        if (template.ProfessionalId != professionalId)
            throw new BusinessRuleException("Texto padrão pertence a outro profissional.");

        return template;
    }

    public static async Task EnsureUniqueTitleAsync(this IPrescriptionTemplateRepository templates,
                                                    uint professionalId, string? title, uint? exceptId,
                                                    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(title))
            return; // required field is validated by the domain

        if (await templates.TitleExistsAsync(professionalId, title, exceptId, cancellationToken))
            throw new BusinessRuleException($"Já existe texto padrão com o título '{title}'.");
    }
}
