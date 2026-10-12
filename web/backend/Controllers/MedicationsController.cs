using Microsoft.AspNetCore.Mvc;
using Prescription.Application.Catalog;
using Prescription.Application.Prescriptions;

namespace Prescription.Api.Controllers;

[Route("api/medications")]
public sealed class MedicationsController : ApiControllerBase
{
    /// <summary>ECF.10.03 b – active medications by name or active ingredient (case and accent insensitive).</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<MedicationDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<MedicationDto>> Search([FromQuery] string? search,
                                                           [FromServices] SearchMedications useCase,
                                                           CancellationToken cancellationToken) =>
        await useCase.ExecuteAsync(search, cancellationToken);

    /// <summary>ECF.10.03 – usage deadline suggested for an item: issue date + days of the medication (null = no limit).</summary>
    [HttpGet("{id}/usage-deadline")]
    [ProducesResponseType<UsageDeadlineResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<UsageDeadlineResponse> SuggestUsageDeadline(uint id, [FromQuery] DateOnly issueDate,
                                                                  [FromServices] SuggestUsageDeadline useCase,
                                                                  CancellationToken cancellationToken) =>
        new(await useCase.ExecuteAsync(id, issueDate, cancellationToken));
}

public sealed record UsageDeadlineResponse(DateOnly? UsageDeadline);
