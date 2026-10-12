using Microsoft.AspNetCore.Mvc;
using Prescription.Application.Catalog;

namespace Prescription.Api.Controllers;

/// <summary>Support lists (pre-populated data).</summary>
[Route("api")]
public sealed class CatalogController : ApiControllerBase
{
    [HttpGet("clients")]
    [ProducesResponseType<IReadOnlyList<ClientDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<ClientDto>> Clients([FromServices] ListClients useCase,
                                                        CancellationToken cancellationToken) =>
        await useCase.ExecuteAsync(cancellationToken);

    /// <summary>Used by the initial professional selection screen (instead of login).</summary>
    [HttpGet("professionals")]
    [ProducesResponseType<IReadOnlyList<ProfessionalDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<ProfessionalDto>> Professionals([FromServices] ListProfessionals useCase,
                                                                    CancellationToken cancellationToken) =>
        await useCase.ExecuteAsync(cancellationToken);

    [HttpGet("institutions")]
    [ProducesResponseType<IReadOnlyList<InstitutionDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<InstitutionDto>> Institutions([FromServices] ListInstitutions useCase,
                                                                  CancellationToken cancellationToken) =>
        await useCase.ExecuteAsync(cancellationToken);

    [HttpGet("pharmaceutical-forms")]
    [ProducesResponseType<IReadOnlyList<LookupDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<LookupDto>> PharmaceuticalForms([FromServices] ListPharmaceuticalForms useCase,
                                                                    CancellationToken cancellationToken) =>
        await useCase.ExecuteAsync(cancellationToken);

    [HttpGet("administration-routes")]
    [ProducesResponseType<IReadOnlyList<LookupDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<LookupDto>> AdministrationRoutes([FromServices] ListAdministrationRoutes useCase,
                                                                     CancellationToken cancellationToken) =>
        await useCase.ExecuteAsync(cancellationToken);

    [HttpGet("dose-units")]
    [ProducesResponseType<IReadOnlyList<DoseUnitDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<DoseUnitDto>> DoseUnits([FromServices] ListDoseUnits useCase,
                                                            CancellationToken cancellationToken) =>
        await useCase.ExecuteAsync(cancellationToken);

    [HttpGet("frequencies")]
    [ProducesResponseType<IReadOnlyList<LookupDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<LookupDto>> Frequencies([FromServices] ListFrequencies useCase,
                                                            CancellationToken cancellationToken) =>
        await useCase.ExecuteAsync(cancellationToken);

    [HttpGet("validity-periods")]
    [ProducesResponseType<IReadOnlyList<ValidityPeriodDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<ValidityPeriodDto>> ValidityPeriods([FromServices] ListValidityPeriods useCase,
                                                                        CancellationToken cancellationToken) =>
        await useCase.ExecuteAsync(cancellationToken);
}
