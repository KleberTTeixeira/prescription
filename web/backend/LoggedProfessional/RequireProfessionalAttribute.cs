using Microsoft.AspNetCore.Mvc;

namespace Prescription.Api.LoggedProfessional;

/// <summary>
/// Endpoint of the logged professional: requires the X-Professional-Id header with an existing professional
/// (chosen on the initial screen, instead of login). Missing, invalid or unknown → 400.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireProfessionalAttribute() : TypeFilterAttribute(typeof(RequireProfessionalFilter));
