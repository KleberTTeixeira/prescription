using System.Globalization;
using Prescription.Blazor.Models;

namespace Prescription.Blazor.Services;

/// <summary>Catalog medications (ECF.10.03).</summary>
public sealed class MedicationApiClient(HttpClient httpClient, ILoggedProfessionalStore loggedProfessional)
    : ApiClientBase(httpClient, loggedProfessional)
{
    /// <summary>Active medications by name or active ingredient (case and accent insensitive).</summary>
    public async Task<IReadOnlyList<Medication>> SearchAsync(string term, CancellationToken cancellationToken = default) =>
        await SendAsync<List<Medication>>(HttpMethod.Get, $"api/medications?search={Uri.EscapeDataString(term)}",
                                          cancellationToken: cancellationToken);

    /// <summary>Usage deadline suggested for an item: issue date + days of the medication (null = no limit).</summary>
    public async Task<DateOnly?> SuggestUsageDeadlineAsync(uint medicationId, DateOnly issueDate,
                                                           CancellationToken cancellationToken = default)
    {
        var date = issueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var response = await SendAsync<UsageDeadlineResponse>(
            HttpMethod.Get, $"api/medications/{medicationId}/usage-deadline?issueDate={date}",
            cancellationToken: cancellationToken);
        return response.UsageDeadline;
    }

    private sealed record UsageDeadlineResponse(DateOnly? UsageDeadline);
}
