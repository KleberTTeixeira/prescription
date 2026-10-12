using Prescription.Application.Abstractions;
using Prescription.Application.Catalog;
using Prescription.Application.Exceptions;
using Prescription.Domain.Entities;

namespace Prescription.Application.Prescriptions;

/// <summary>
/// Shared by create and update: checks that the referenced records exist and builds the domain items.
/// Catalog medications must be active; their name and pharmaceutical form come from the catalog.
/// </summary>
public sealed class PrescriptionInputResolver(IProfessionalRepository professionals, IClientRepository clients,
                                              IInstitutionRepository institutions, IMedicationRepository medications,
                                              ILookupRepository lookups)
{
    public async Task EnsureProfessionalExistsAsync(uint professionalId, CancellationToken cancellationToken)
    {
        if (await professionals.GetByIdAsync(professionalId, cancellationToken) is null)
            throw new BusinessRuleException($"Profissional {professionalId} não encontrado.");
    }

    public async Task<IReadOnlyList<PrescriptionItem>> ResolveAsync(PrescriptionRequest request,
                                                                    CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await clients.GetByIdAsync(request.ClientId, cancellationToken) is null)
            throw new BusinessRuleException($"Cliente {request.ClientId} não encontrado.");

        if (await institutions.GetByIdAsync(request.InstitutionId, cancellationToken) is null)
            throw new BusinessRuleException($"Instituição {request.InstitutionId} não encontrada.");

        var itemRequests = request.Items ?? [];
        var catalog = await LoadMedicationsAsync(itemRequests, cancellationToken);

        var items = new List<PrescriptionItem>(itemRequests.Count);
        foreach (var itemRequest in itemRequests)
            items.Add(await BuildItemAsync(itemRequest, catalog, cancellationToken));

        return items;
    }

    private async Task<Dictionary<uint, Medication>> LoadMedicationsAsync(
        IReadOnlyList<PrescriptionItemRequest> itemRequests, CancellationToken cancellationToken)
    {
        var ids = itemRequests.Where(i => i?.MedicationId is not null)
                              .Select(i => i.MedicationId!.Value)
                              .Distinct()
                              .ToList();

        if (ids.Count == 0)
            return [];

        return (await medications.GetByIdsAsync(ids, cancellationToken)).ToDictionary(m => m.Id);
    }

    private async Task<PrescriptionItem> BuildItemAsync(PrescriptionItemRequest? request,
                                                        Dictionary<uint, Medication> catalog,
                                                        CancellationToken cancellationToken)
    {
        if (request is null)
            throw new BusinessRuleException("Item da receita inválido.");

        string? medicationName;
        uint pharmaceuticalFormId;

        if (request.MedicationId is { } medicationId)
        {
            if (!catalog.TryGetValue(medicationId, out var medication))
                throw new BusinessRuleException($"Medicamento {medicationId} não encontrado.");

            if (!medication.IsActive)
                throw new BusinessRuleException($"Medicamento '{medication.DisplayName()}' está inativo.");

            medicationName = medication.DisplayName();
            pharmaceuticalFormId = medication.PharmaceuticalFormId;
        }
        else
        {
            medicationName = request.MedicationName;
            pharmaceuticalFormId = request.PharmaceuticalFormId ?? 0;

            if (pharmaceuticalFormId != 0 &&
                !await lookups.PharmaceuticalFormExistsAsync(pharmaceuticalFormId, cancellationToken))
                throw new BusinessRuleException($"Forma farmacêutica {pharmaceuticalFormId} não encontrada.");
        }

        if (request.DoseUnitId != 0 && !await lookups.DoseUnitExistsAsync(request.DoseUnitId, cancellationToken))
            throw new BusinessRuleException($"Unidade de dose {request.DoseUnitId} não encontrada.");

        if (request.FrequencyId != 0 && !await lookups.FrequencyExistsAsync(request.FrequencyId, cancellationToken))
            throw new BusinessRuleException($"Frequência {request.FrequencyId} não encontrada.");

        if (request.AdministrationRouteId != 0 &&
            !await lookups.AdministrationRouteExistsAsync(request.AdministrationRouteId, cancellationToken))
            throw new BusinessRuleException($"Via de administração {request.AdministrationRouteId} não encontrada.");

        // Required fields (id 0, empty name, dose etc.) are validated by the domain.
        return new PrescriptionItem(request.MedicationId, medicationName!, pharmaceuticalFormId, request.Dose,
                                    request.DoseUnitId, request.FrequencyId, request.StartDate, request.DurationDays,
                                    request.IsContinuousUse, request.AdministrationRouteId, request.UsageDeadline,
                                    request.Notes);
    }
}
