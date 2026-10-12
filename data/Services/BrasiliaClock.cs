using Prescription.Application.Abstractions;

namespace Prescription.Data.Services;

/// <summary>Current date/time in Brasília time (America/Sao_Paulo), regardless of the server time zone (UTC in Docker).</summary>
internal sealed class BrasiliaClock(TimeProvider timeProvider) : IClock
{
    private static readonly TimeZoneInfo Brasilia = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public DateTime Now => TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), Brasilia).DateTime;
}
