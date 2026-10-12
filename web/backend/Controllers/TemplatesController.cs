using Microsoft.AspNetCore.Mvc;
using Prescription.Api.LoggedProfessional;
using Prescription.Application.Templates;

namespace Prescription.Api.Controllers;

/// <summary>ECF.10.02 – standard texts of the logged professional.</summary>
[Route("api/templates")]
[RequireProfessional]
public sealed class TemplatesController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TemplateDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<TemplateDto>> List([FromServices] ListTemplates useCase,
                                                       CancellationToken cancellationToken) =>
        await useCase.ExecuteAsync(ProfessionalId, cancellationToken);

    [HttpGet("{id}")]
    [ProducesResponseType<TemplateDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<TemplateDto> Get(uint id, [FromServices] GetTemplate useCase,
                                       CancellationToken cancellationToken) =>
        await useCase.ExecuteAsync(ProfessionalId, id, cancellationToken);

    [HttpPost]
    [ProducesResponseType<TemplateDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TemplateDto>> Create(TemplateRequest request, [FromServices] CreateTemplate useCase,
                                                        CancellationToken cancellationToken)
    {
        var created = await useCase.ExecuteAsync(ProfessionalId, request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [ProducesResponseType<TemplateDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<TemplateDto> Update(uint id, TemplateRequest request, [FromServices] UpdateTemplate useCase,
                                          CancellationToken cancellationToken) =>
        await useCase.ExecuteAsync(ProfessionalId, id, request, cancellationToken);

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(uint id, [FromServices] DeleteTemplate useCase,
                                            CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(ProfessionalId, id, cancellationToken);
        return NoContent();
    }
}
