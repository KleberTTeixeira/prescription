namespace Prescription.Blazor.Services;

/// <summary>
/// Today in Brasília time (the API records dates in America/Sao_Paulo). The Blazor server may run in another
/// time zone (UTC in Docker), so the local date of the server is not used.
/// </summary>
public interface IClock
{
    DateOnly Today { get; }
}

public sealed class BrasiliaClock : IClock
{
    private static readonly TimeZoneInfo Brasilia = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Brasilia));
}
