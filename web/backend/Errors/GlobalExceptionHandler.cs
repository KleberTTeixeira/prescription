using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Prescription.Application.Exceptions;
using Prescription.Domain.Exceptions;

namespace Prescription.Api.Errors;

/// <summary>
/// Converts exceptions into ProblemDetails (RFC 7807): business rule → 400, not found → 404,
/// anything else → 500 without internal details. The reason goes in "detail".
/// </summary>
internal sealed class GlobalExceptionHandler(IProblemDetailsService problemDetails,
                                             ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
                                                CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            DomainException or BusinessRuleException =>
                (StatusCodes.Status400BadRequest, "Requisição inválida", exception.Message),
            NotFoundException =>
                (StatusCodes.Status404NotFound, "Não encontrado", exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno", "Ocorreu um erro inesperado.")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception");

        httpContext.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = status, Title = title, Detail = detail }
        });
    }
}
