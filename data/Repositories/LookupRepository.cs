using Microsoft.EntityFrameworkCore;
using Prescription.Application.Abstractions;
using Prescription.Data.Context;
using Prescription.Domain.Entities;

namespace Prescription.Data.Repositories;

/// <summary>Lookup tables, ordered by id (keeps the logical order of the seed).</summary>
internal sealed class LookupRepository(PrescriptionDbContext context) : ILookupRepository
{
    public async Task<IReadOnlyList<PharmaceuticalForm>> ListPharmaceuticalFormsAsync(
        CancellationToken cancellationToken = default) =>
        await context.PharmaceuticalForms.AsNoTracking().OrderBy(x => x.Id).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AdministrationRoute>> ListAdministrationRoutesAsync(
        CancellationToken cancellationToken = default) =>
        await context.AdministrationRoutes.AsNoTracking().OrderBy(x => x.Id).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DoseUnit>> ListDoseUnitsAsync(CancellationToken cancellationToken = default) =>
        await context.DoseUnits.AsNoTracking().OrderBy(x => x.Id).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Frequency>> ListFrequenciesAsync(CancellationToken cancellationToken = default) =>
        await context.Frequencies.AsNoTracking().OrderBy(x => x.Id).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ValidityPeriod>> ListValidityPeriodsAsync(
        CancellationToken cancellationToken = default) =>
        await context.ValidityPeriods.AsNoTracking().OrderBy(x => x.Id).ToListAsync(cancellationToken);

    public Task<bool> PharmaceuticalFormExistsAsync(uint id, CancellationToken cancellationToken = default) =>
        context.PharmaceuticalForms.AnyAsync(x => x.Id == id, cancellationToken);

    public Task<bool> AdministrationRouteExistsAsync(uint id, CancellationToken cancellationToken = default) =>
        context.AdministrationRoutes.AnyAsync(x => x.Id == id, cancellationToken);

    public Task<bool> DoseUnitExistsAsync(uint id, CancellationToken cancellationToken = default) =>
        context.DoseUnits.AnyAsync(x => x.Id == id, cancellationToken);

    public Task<bool> FrequencyExistsAsync(uint id, CancellationToken cancellationToken = default) =>
        context.Frequencies.AnyAsync(x => x.Id == id, cancellationToken);
}
