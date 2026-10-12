using Prescription.Domain.Entities;

namespace Prescription.Application.Abstractions;

/// <summary>Lookup tables: pharmaceutical forms, administration routes, dose units, frequencies, validity periods.</summary>
public interface ILookupRepository
{
    Task<IReadOnlyList<PharmaceuticalForm>> ListPharmaceuticalFormsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdministrationRoute>> ListAdministrationRoutesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DoseUnit>> ListDoseUnitsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Frequency>> ListFrequenciesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ValidityPeriod>> ListValidityPeriodsAsync(CancellationToken cancellationToken = default);

    Task<bool> PharmaceuticalFormExistsAsync(uint id, CancellationToken cancellationToken = default);

    Task<bool> AdministrationRouteExistsAsync(uint id, CancellationToken cancellationToken = default);

    Task<bool> DoseUnitExistsAsync(uint id, CancellationToken cancellationToken = default);

    Task<bool> FrequencyExistsAsync(uint id, CancellationToken cancellationToken = default);
}
