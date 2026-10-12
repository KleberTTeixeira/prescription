using Prescription.Application.Abstractions;

namespace Prescription.Tests.Application.Fakes;

internal sealed class FakeClock(DateTime now) : IClock
{
    public DateTime Now { get; set; } = now;
}
