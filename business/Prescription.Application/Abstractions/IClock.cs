namespace Prescription.Application.Abstractions;

/// <summary>Current date/time in Brasília time (America/Sao_Paulo).</summary>
public interface IClock
{
    DateTime Now { get; }

    DateOnly Today => DateOnly.FromDateTime(Now);
}
