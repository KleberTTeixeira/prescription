using Microsoft.AspNetCore.Mvc;
using Prescription.Api.LoggedProfessional;
using Prescription.Application.Catalog;
using Prescription.Application.Favorites;

namespace Prescription.Api.Controllers;

/// <summary>ECF.10.10 – favorite medications of the logged professional.</summary>
[Route("api/favorites")]
[RequireProfessional]
public sealed class FavoritesController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<MedicationDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<MedicationDto>> List([FromServices] ListFavorites useCase,
                                                         CancellationToken cancellationToken) =>
        await useCase.ExecuteAsync(ProfessionalId, cancellationToken);

    /// <summary>Adds a favorite (already favorite: no error).</summary>
    [HttpPut("{medicationId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Add(uint medicationId, [FromServices] AddFavorite useCase,
                                         CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(ProfessionalId, medicationId, cancellationToken);
        return NoContent();
    }

    /// <summary>Removes a favorite (not a favorite: no error).</summary>
    [HttpDelete("{medicationId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remove(uint medicationId, [FromServices] RemoveFavorite useCase,
                                            CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(ProfessionalId, medicationId, cancellationToken);
        return NoContent();
    }
}
