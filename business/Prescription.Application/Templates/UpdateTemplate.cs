using Prescription.Application.Abstractions;

namespace Prescription.Application.Templates;

/// <summary>ECF.10.02 – changes an own standard text.</summary>
public sealed class UpdateTemplate(IPrescriptionTemplateRepository templates, IUnitOfWork unitOfWork, IClock clock)
{
    public async Task<TemplateDto> ExecuteAsync(uint professionalId, uint id, TemplateRequest request,
                                                CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var template = await templates.GetOwnAsync(professionalId, id, cancellationToken);
        await templates.EnsureUniqueTitleAsync(professionalId, request.Title, id, cancellationToken);

        template.Update(request.Title, request.Content, clock.Now);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return template.ToDto();
    }
}
