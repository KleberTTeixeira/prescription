using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Prescription.Application.Abstractions;

namespace Prescription.Api.LoggedProfessional;

internal sealed class RequireProfessionalFilter(IProfessionalRepository professionals) : IAsyncActionFilter
{
    public const string HeaderName = "X-Professional-Id";
    private const string ItemKey = "ProfessionalId";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var header = context.HttpContext.Request.Headers[HeaderName].ToString();

        if (string.IsNullOrWhiteSpace(header))
        {
            context.Result = BadRequest($"Cabeçalho {HeaderName} não informado.");
            return;
        }

        if (!uint.TryParse(header, out var professionalId) || professionalId == 0)
        {
            context.Result = BadRequest($"Cabeçalho {HeaderName} inválido.");
            return;
        }

        if (await professionals.GetByIdAsync(professionalId, context.HttpContext.RequestAborted) is null)
        {
            context.Result = BadRequest($"Profissional {professionalId} não encontrado.");
            return;
        }

        context.HttpContext.Items[ItemKey] = professionalId;
        await next();
    }

    public static uint GetProfessionalId(HttpContext httpContext) =>
        httpContext.Items[ItemKey] as uint?
        ?? throw new InvalidOperationException($"Endpoint sem [{nameof(RequireProfessionalAttribute)}].");

    private static ObjectResult BadRequest(string detail) =>
        new(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = "Requisição inválida", Detail = detail })
        {
            StatusCode = StatusCodes.Status400BadRequest,
            ContentTypes = { "application/problem+json" }
        };
}
