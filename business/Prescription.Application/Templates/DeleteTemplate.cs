using Prescription.Application.Abstractions;

namespace Prescription.Application.Templates;

/// <summary>ECF.10.02 – deletes an own standard text.</summary>
public sealed class DeleteTemplate(IPrescriptionTemplateRepository templates, IUnitOfWork unitOfWork)
{
    public async Task ExecuteAsync(uint professionalId, uint id, CancellationToken cancellationToken = default)
    {
        var template = await templates.GetOwnAsync(professionalId, id, cancellationToken);

        templates.Remove(template);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
