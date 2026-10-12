using Prescription.Application.Abstractions;

namespace Prescription.Application.Templates;

/// <summary>ECF.10.02 – standard texts of the logged professional.</summary>
public sealed class ListTemplates(IPrescriptionTemplateRepository templates)
{
    public async Task<IReadOnlyList<TemplateDto>> ExecuteAsync(uint professionalId,
                                                               CancellationToken cancellationToken = default) =>
        (await templates.ListByProfessionalAsync(professionalId, cancellationToken)).Select(t => t.ToDto()).ToList();
}
