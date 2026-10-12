using Prescription.Application.Abstractions;
using Prescription.Application.Exceptions;
using Prescription.Domain.Entities;

namespace Prescription.Application.Templates;

/// <summary>ECF.10.02 a – creates a standard text with title for the logged professional.</summary>
public sealed class CreateTemplate(IPrescriptionTemplateRepository templates, IProfessionalRepository professionals,
                                   IUnitOfWork unitOfWork, IClock clock)
{
    public async Task<TemplateDto> ExecuteAsync(uint professionalId, TemplateRequest request,
                                                CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await professionals.GetByIdAsync(professionalId, cancellationToken) is null)
            throw new BusinessRuleException($"Profissional {professionalId} não encontrado.");

        await templates.EnsureUniqueTitleAsync(professionalId, request.Title, null, cancellationToken);

        var template = new PrescriptionTemplate(professionalId, request.Title, request.Content, clock.Now);
        templates.Add(template);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return template.ToDto();
    }
}
