using Prescription.Blazor.Models;

namespace Prescription.Blazor.Services;

/// <summary>ECF.10.02 – standard texts of the logged professional.</summary>
public sealed class TemplateApiClient(HttpClient httpClient, ILoggedProfessionalStore loggedProfessional)
    : ApiClientBase(httpClient, loggedProfessional)
{
    public async Task<IReadOnlyList<Template>> ListAsync(CancellationToken cancellationToken = default) =>
        await SendAsync<List<Template>>(HttpMethod.Get, "api/templates", cancellationToken: cancellationToken);

    public Task<Template> GetAsync(uint id, CancellationToken cancellationToken = default) =>
        SendAsync<Template>(HttpMethod.Get, $"api/templates/{id}", cancellationToken: cancellationToken);

    public Task<Template> CreateAsync(TemplateRequest request, CancellationToken cancellationToken = default) =>
        SendAsync<Template>(HttpMethod.Post, "api/templates", request, cancellationToken);

    public Task<Template> UpdateAsync(uint id, TemplateRequest request, CancellationToken cancellationToken = default) =>
        SendAsync<Template>(HttpMethod.Put, $"api/templates/{id}", request, cancellationToken);

    public Task DeleteAsync(uint id, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Delete, $"api/templates/{id}", cancellationToken: cancellationToken);
}
