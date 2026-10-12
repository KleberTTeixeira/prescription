using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Prescription.Api.LoggedProfessional;
using Prescription.Application.Prescriptions;
using Prescription.Domain.Enums;

namespace Prescription.Api.Controllers;

/// <summary>
/// Prescription CRUD (the list with OData is in PrescriptionsODataController).
/// [ODataIgnored]: the controller name matches the OData entity set "Prescriptions"; without it the OData
/// conventions also map this controller to GET odata/Prescriptions (conflict found in the API tests).
/// </summary>
[Route("api/prescriptions")]
[ODataIgnored]
public sealed class PrescriptionsController : ApiControllerBase
{
    /// <summary>Views a prescription (any professional).</summary>
    [HttpGet("{id}")]
    [ProducesResponseType<PrescriptionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<PrescriptionDto> Get(uint id, [FromServices] GetPrescription useCase,
                                           CancellationToken cancellationToken) =>
        await useCase.ExecuteAsync(id, cancellationToken);

    /// <summary>Creates a prescription of the logged professional (ECF.10.01 / ECF.10.03).</summary>
    [HttpPost]
    [RequireProfessional]
    [ProducesResponseType<PrescriptionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PrescriptionDto>> Create(PrescriptionRequest request,
                                                            [FromServices] CreatePrescription useCase,
                                                            CancellationToken cancellationToken)
    {
        var created = await useCase.ExecuteAsync(ProfessionalId, request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>Changes a not issued prescription of the logged professional.</summary>
    [HttpPut("{id}")]
    [RequireProfessional]
    [ProducesResponseType<PrescriptionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<PrescriptionDto> Update(uint id, PrescriptionRequest request,
                                              [FromServices] UpdatePrescription useCase,
                                              CancellationToken cancellationToken) =>
        await useCase.ExecuteAsync(ProfessionalId, id, request, cancellationToken);

    /// <summary>Deletes a prescription of the logged professional.</summary>
    [HttpDelete("{id}")]
    [RequireProfessional]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(uint id, [FromServices] DeletePrescription useCase,
                                            CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(ProfessionalId, id, cancellationToken);
        return NoContent();
    }

    /// <summary>ECF.10.04 – print data (any professional can reprint).</summary>
    [HttpGet("{id}/print-data")]
    [ProducesResponseType<PrescriptionPrintDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<PrescriptionPrintDto> GetPrintData(uint id, [FromServices] GetPrescriptionPrintData useCase,
                                                         CancellationToken cancellationToken) =>
        await useCase.ExecuteAsync(id, cancellationToken);

    /// <summary>Called after the user confirms the print: issues the prescription (it becomes locked).</summary>
    [HttpPost("{id}/print-confirmation")]
    [RequireProfessional]
    [ProducesResponseType<PrescriptionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<PrescriptionDto> ConfirmPrint(uint id, [FromServices] MarkPrescriptionAsPrinted useCase,
                                                    CancellationToken cancellationToken) =>
        await useCase.ExecuteAsync(ProfessionalId, id, cancellationToken);

    /// <summary>Category suggested from the catalog medications (the user can change it).</summary>
    [HttpGet("suggested-category")]
    [ProducesResponseType<SuggestedCategoryResponse>(StatusCodes.Status200OK)]
    public async Task<SuggestedCategoryResponse> SuggestCategory([FromQuery] uint[]? medicationIds,
                                                                 [FromServices] SuggestPrescriptionCategory useCase,
                                                                 CancellationToken cancellationToken) =>
        new(await useCase.ExecuteAsync(medicationIds, cancellationToken));
}

public sealed record SuggestedCategoryResponse(PrescriptionCategory Category);
