using Prescription.Blazor.Services;

namespace Prescription.Blazor.Tests.Fakes;

public sealed class FakeClock(DateOnly today) : IClock
{
    public DateOnly Today { get; } = today;
}
