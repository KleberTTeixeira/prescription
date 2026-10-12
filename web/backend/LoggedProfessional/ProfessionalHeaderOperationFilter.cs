using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Prescription.Api.LoggedProfessional;

/// <summary>Shows the X-Professional-Id header field in Swagger UI on the endpoints that require it.</summary>
internal sealed class ProfessionalHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var requires = context.MethodInfo.GetCustomAttributes(true).OfType<RequireProfessionalAttribute>().Any()
                       || (context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                                  .OfType<RequireProfessionalAttribute>().Any() ?? false);
        if (!requires)
            return;

        operation.Parameters ??= [];
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = RequireProfessionalFilter.HeaderName,
            In = ParameterLocation.Header,
            Required = true,
            Description = "Id do profissional logado (escolhido na tela inicial).",
            Schema = new OpenApiSchema { Type = "integer", Format = "int64" }
        });
    }
}
