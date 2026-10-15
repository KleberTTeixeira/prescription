using Prescription.Blazor.Models;

namespace Prescription.Blazor.Services;

/// <summary>Support lists of the Web API (/api/...).</summary>
public sealed class CatalogApiClient(HttpClient httpClient, ILoggedProfessionalStore loggedProfessional)
    : ApiClientBase(httpClient, loggedProfessional)
{
    /// <summary>Used by the initial professional selection screen (instead of login).</summary>
    public Task<IReadOnlyList<Professional>> GetProfessionalsAsync(CancellationToken cancellationToken = default) =>
        ListAsync<Professional>("api/professionals", cancellationToken);

    public Task<IReadOnlyList<Client>> GetClientsAsync(CancellationToken cancellationToken = default) =>
        ListAsync<Client>("api/clients", cancellationToken);

    public Task<IReadOnlyList<Institution>> GetInstitutionsAsync(CancellationToken cancellationToken = default) =>
        ListAsync<Institution>("api/institutions", cancellationToken);

    public Task<IReadOnlyList<Lookup>> GetPharmaceuticalFormsAsync(CancellationToken cancellationToken = default) =>
        ListAsync<Lookup>("api/pharmaceutical-forms", cancellationToken);

    public Task<IReadOnlyList<Lookup>> GetAdministrationRoutesAsync(CancellationToken cancellationToken = default) =>
        ListAsync<Lookup>("api/administration-routes", cancellationToken);

    public Task<IReadOnlyList<DoseUnit>> GetDoseUnitsAsync(CancellationToken cancellationToken = default) =>
        ListAsync<DoseUnit>("api/dose-units", cancellationToken);

    public Task<IReadOnlyList<Lookup>> GetFrequenciesAsync(CancellationToken cancellationToken = default) =>
        ListAsync<Lookup>("api/frequencies", cancellationToken);

    public Task<IReadOnlyList<ValidityPeriod>> GetValidityPeriodsAsync(CancellationToken cancellationToken = default) =>
        ListAsync<ValidityPeriod>("api/validity-periods", cancellationToken);

    private async Task<IReadOnlyList<T>> ListAsync<T>(string url, CancellationToken cancellationToken) =>
        await SendAsync<List<T>>(HttpMethod.Get, url, cancellationToken: cancellationToken);
}
