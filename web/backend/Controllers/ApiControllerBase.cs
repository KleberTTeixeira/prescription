using Microsoft.AspNetCore.Mvc;
using Prescription.Api.LoggedProfessional;

namespace Prescription.Api.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Professional of the X-Professional-Id header (endpoints with [RequireProfessional]).</summary>
    protected uint ProfessionalId => RequireProfessionalFilter.GetProfessionalId(HttpContext);
}
