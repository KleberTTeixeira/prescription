using Prescription.Application.Abstractions;

namespace Prescription.Application.Templates;

/// <summary>ECF.10.02 b – standard text used as base for a prescription (only own texts).</summary>
public sealed class GetTemplate(IPrescriptionTemplateRepository templates)
{
    public async Task<TemplateDto> ExecuteAsync(uint professionalId, uint id,
                                                CancellationToken cancellationToken = default) =>
        (await templates.GetOwnAsync(professionalId, id, cancellationToken)).ToDto();
}
