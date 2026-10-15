using Prescription.Blazor.Models;

namespace Prescription.Blazor.Services;

/// <summary>ECF.10.10 – favorite medications of the logged professional.</summary>
public sealed class FavoriteApiClient(HttpClient httpClient, ILoggedProfessionalStore loggedProfessional)
    : ApiClientBase(httpClient, loggedProfessional)
{
    public async Task<IReadOnlyList<Medication>> ListAsync(CancellationToken cancellationToken = default) =>
        await SendAsync<List<Medication>>(HttpMethod.Get, "api/favorites", cancellationToken: cancellationToken);

    /// <summary>Already favorite: no error.</summary>
    public Task AddAsync(uint medicationId, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Put, $"api/favorites/{medicationId}", cancellationToken: cancellationToken);

    /// <summary>Not a favorite: no error.</summary>
    public Task RemoveAsync(uint medicationId, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Delete, $"api/favorites/{medicationId}", cancellationToken: cancellationToken);
}
