using System.Text.Json.Serialization;
using Prescription.Blazor.Models;

namespace Prescription.Blazor.Services;

/// <summary>Prescriptions of the logged professional (list with OData; CRUD, print and suggestion in /api).</summary>
public sealed class PrescriptionApiClient(HttpClient httpClient, ILoggedProfessionalStore loggedProfessional)
    : ApiClientBase(httpClient, loggedProfessional)
{
    public async Task<PrescriptionPage> ListAsync(PrescriptionListQuery query, CancellationToken cancellationToken = default)
    {
        var parameters = new List<string>
        {
            "$count=true",
            $"$orderby={Uri.EscapeDataString(ToOrderBy(query.Sort))}",
            $"$top={query.Top}",
            $"$skip={query.Skip}"
        };
        var clientName = query.ClientName.Trim();
        if (clientName.Length > 0)
        {
            var filter = $"contains(ClientName,'{clientName.Replace("'", "''")}')";
            parameters.Add($"$filter={Uri.EscapeDataString(filter)}");
        }

        var response = await SendAsync<ODataResponse>(HttpMethod.Get, $"odata/Prescriptions?{string.Join('&', parameters)}",
                                                      cancellationToken: cancellationToken);
        var rows = response.Value
            .Select(row => new PrescriptionListItem(row.Id, row.ClientName, row.InstitutionName, row.Format,
                                                    row.Category, row.IssuedAt.DateTime, row.IsIssued, row.ItemCount))
            .ToList();
        return new PrescriptionPage(rows, response.Count ?? rows.Count);
    }

    public Task<MedicalPrescription> GetAsync(uint id, CancellationToken cancellationToken = default) =>
        SendAsync<MedicalPrescription>(HttpMethod.Get, $"api/prescriptions/{id}", cancellationToken: cancellationToken);

    public Task<MedicalPrescription> CreateAsync(PrescriptionRequest request, CancellationToken cancellationToken = default) =>
        SendAsync<MedicalPrescription>(HttpMethod.Post, "api/prescriptions", request, cancellationToken);

    public Task<MedicalPrescription> UpdateAsync(uint id, PrescriptionRequest request,
                                                 CancellationToken cancellationToken = default) =>
        SendAsync<MedicalPrescription>(HttpMethod.Put, $"api/prescriptions/{id}", request, cancellationToken);

    public Task DeleteAsync(uint id, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Delete, $"api/prescriptions/{id}", cancellationToken: cancellationToken);

    /// <summary>ECF.10.04 print data (any professional can reprint).</summary>
    public Task<PrescriptionPrintData> GetPrintDataAsync(uint id, CancellationToken cancellationToken = default) =>
        SendAsync<PrescriptionPrintData>(HttpMethod.Get, $"api/prescriptions/{id}/print-data",
                                         cancellationToken: cancellationToken);

    /// <summary>Issues the prescription after the user confirms the print (it becomes locked).</summary>
    public Task<MedicalPrescription> ConfirmPrintAsync(uint id, CancellationToken cancellationToken = default) =>
        SendAsync<MedicalPrescription>(HttpMethod.Post, $"api/prescriptions/{id}/print-confirmation",
                                       cancellationToken: cancellationToken);

    /// <summary>Category suggested from the catalog medications (the user can change it).</summary>
    public async Task<PrescriptionCategory> SuggestCategoryAsync(IEnumerable<uint> medicationIds,
                                                                 CancellationToken cancellationToken = default)
    {
        var query = string.Join('&', medicationIds.Select(id => $"medicationIds={id}"));
        var url = query.Length > 0 ? $"api/prescriptions/suggested-category?{query}" : "api/prescriptions/suggested-category";
        var response = await SendAsync<SuggestedCategoryResponse>(HttpMethod.Get, url, cancellationToken: cancellationToken);
        return response.Category;
    }

    /// <summary>Ties are ordered by Id (same direction) so paging does not repeat or skip rows.</summary>
    private static string ToOrderBy(PrescriptionSort sort)
    {
        var direction = sort.Direction == PrescriptionSortDirection.Asc ? "asc" : "desc";
        var orderBy = $"{sort.Field} {direction}";
        return sort.Field == PrescriptionSortField.Id ? orderBy : $"{orderBy},Id {direction}";
    }

    /// <summary>GET /odata/Prescriptions: C# property names; dates with the Brasília offset ("...-03:00").</summary>
    private sealed record ODataResponse([property: JsonPropertyName("@odata.count")] int? Count,
                                        IReadOnlyList<ODataRow> Value);

    /// <summary>IssuedAt.DateTime keeps the Brasília time as written (no conversion to the server time zone).</summary>
    private sealed record ODataRow(uint Id, string ClientName, string InstitutionName, PrescriptionFormat Format,
                                   PrescriptionCategory Category, DateTimeOffset IssuedAt, bool IsIssued,
                                   int ItemCount);

    private sealed record SuggestedCategoryResponse(PrescriptionCategory Category);
}
