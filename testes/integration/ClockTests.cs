using Microsoft.Extensions.DependencyInjection;
using Prescription.Application.Abstractions;
using Prescription.Data;

namespace Prescription.IntegrationTests;

/// <summary>Brasília clock (no database needed). Brazil has no daylight saving time since 2019: UTC-3.</summary>
public class ClockTests
{
    [Fact]
    public void Clock_ReturnsBrasiliaTime()
    {
        using var provider = new ServiceCollection().AddData("server=unused").BuildServiceProvider();
        var clock = provider.GetRequiredService<IClock>();

        var difference = DateTime.UtcNow - clock.Now;

        Assert.InRange(difference.TotalMinutes, 179, 181);
    }
}
