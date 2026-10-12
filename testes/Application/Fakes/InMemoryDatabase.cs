using Prescription.Application.Abstractions;
using Prescription.Domain.Entities;

namespace Prescription.Tests.Application.Fakes;

/// <summary>
/// Hand written fake of all repositories and the unit of work. Adds and removes stay pending until
/// SaveChangesAsync (like EF Core); reads load the navigations (like Include).
/// </summary>
internal sealed class InMemoryDatabase :
    IPrescriptionRepository, IPrescriptionTemplateRepository, IFavoriteMedicationRepository,
    IClientRepository, IProfessionalRepository, IInstitutionRepository, IMedicationRepository,
    ILookupRepository, IUnitOfWork
{
    public List<Client> Clients { get; } = [];
    public List<Professional> Professionals { get; } = [];
    public List<Institution> Institutions { get; } = [];
    public List<Medication> Medications { get; } = [];
    public List<PharmaceuticalForm> PharmaceuticalForms { get; } = [];
    public List<AdministrationRoute> AdministrationRoutes { get; } = [];
    public List<DoseUnit> DoseUnits { get; } = [];
    public List<Frequency> Frequencies { get; } = [];
    public List<ValidityPeriod> ValidityPeriods { get; } = [];
    public List<MedicalPrescription> Prescriptions { get; } = [];
    public List<PrescriptionTemplate> Templates { get; } = [];
    public List<FavoriteMedication> Favorites { get; } = [];

    public int SaveCount { get; private set; }

    private readonly List<object> _pendingAdds = [];
    private readonly List<object> _pendingRemoves = [];
    private uint _nextId = 1000;

    // ---- Unit of work ----

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entity in _pendingAdds)
        {
            switch (entity)
            {
                case MedicalPrescription p:
                    p.WithId(_nextId++);
                    foreach (var item in p.Items)
                        item.WithId(_nextId++).With("PrescriptionId", p.Id);
                    Prescriptions.Add(p);
                    break;
                case PrescriptionTemplate t:
                    Templates.Add(t.WithId(_nextId++));
                    break;
                case FavoriteMedication f:
                    Favorites.Add(f);
                    break;
            }
        }

        foreach (var entity in _pendingRemoves)
        {
            switch (entity)
            {
                case MedicalPrescription p: Prescriptions.Remove(p); break;
                case PrescriptionTemplate t: Templates.Remove(t); break;
                case FavoriteMedication f: Favorites.Remove(f); break;
            }
        }

        // New items added to existing prescriptions get ids.
        foreach (var item in Prescriptions.SelectMany(p => p.Items).Where(i => i.Id == 0))
            item.WithId(_nextId++);

        _pendingAdds.Clear();
        _pendingRemoves.Clear();
        SaveCount++;
        return Task.CompletedTask;
    }

    // ---- Prescriptions ----

    IQueryable<MedicalPrescription> IPrescriptionRepository.Query() =>
        Prescriptions.Select(Load).ToList().AsQueryable();

    Task<MedicalPrescription?> IPrescriptionRepository.GetByIdAsync(uint id, CancellationToken cancellationToken)
    {
        var prescription = Prescriptions.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(prescription is null ? null : Load(prescription));
    }

    void IPrescriptionRepository.Add(MedicalPrescription prescription) => _pendingAdds.Add(prescription);

    void IPrescriptionRepository.Remove(MedicalPrescription prescription) => _pendingRemoves.Add(prescription);

    private MedicalPrescription Load(MedicalPrescription p)
    {
        p.With("Client", Clients.Single(c => c.Id == p.ClientId))
         .With("Professional", Professionals.Single(x => x.Id == p.ProfessionalId))
         .With("Institution", Institutions.Single(i => i.Id == p.InstitutionId));

        foreach (var item in p.Items)
        {
            item.With("Medication", Medications.SingleOrDefault(m => m.Id == item.MedicationId))
                .With("PharmaceuticalForm", PharmaceuticalForms.Single(f => f.Id == item.PharmaceuticalFormId))
                .With("DoseUnit", DoseUnits.Single(u => u.Id == item.DoseUnitId))
                .With("Frequency", Frequencies.Single(f => f.Id == item.FrequencyId))
                .With("AdministrationRoute", AdministrationRoutes.Single(r => r.Id == item.AdministrationRouteId));
        }

        return p;
    }

    // ---- Templates ----

    Task<PrescriptionTemplate?> IPrescriptionTemplateRepository.GetByIdAsync(uint id, CancellationToken cancellationToken) =>
        Task.FromResult(Templates.FirstOrDefault(t => t.Id == id));

    Task<IReadOnlyList<PrescriptionTemplate>> IPrescriptionTemplateRepository.ListByProfessionalAsync(
        uint professionalId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<PrescriptionTemplate>>(
            Templates.Where(t => t.ProfessionalId == professionalId).OrderBy(t => t.Title).ToList());

    Task<bool> IPrescriptionTemplateRepository.TitleExistsAsync(uint professionalId, string title, uint? exceptTemplateId,
                                                                CancellationToken cancellationToken) =>
        Task.FromResult(Templates.Any(t => t.ProfessionalId == professionalId
                                           && string.Equals(t.Title, title, StringComparison.OrdinalIgnoreCase)
                                           && t.Id != exceptTemplateId));

    void IPrescriptionTemplateRepository.Add(PrescriptionTemplate template) => _pendingAdds.Add(template);

    void IPrescriptionTemplateRepository.Remove(PrescriptionTemplate template) => _pendingRemoves.Add(template);

    // ---- Favorites ----

    Task<FavoriteMedication?> IFavoriteMedicationRepository.GetAsync(uint professionalId, uint medicationId,
                                                                     CancellationToken cancellationToken) =>
        Task.FromResult(Favorites.FirstOrDefault(f => f.ProfessionalId == professionalId && f.MedicationId == medicationId));

    Task<IReadOnlyList<FavoriteMedication>> IFavoriteMedicationRepository.ListActiveByProfessionalAsync(
        uint professionalId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<FavoriteMedication>>(
            Favorites.Where(f => f.ProfessionalId == professionalId)
                     .Select(f => f.With("Medication", Medications.Single(m => m.Id == f.MedicationId)))
                     .Where(f => f.Medication.IsActive)
                     .ToList());

    void IFavoriteMedicationRepository.Add(FavoriteMedication favorite) => _pendingAdds.Add(favorite);

    void IFavoriteMedicationRepository.Remove(FavoriteMedication favorite) => _pendingRemoves.Add(favorite);

    // ---- Catalog ----

    Task<IReadOnlyList<Client>> IClientRepository.ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Client>>(Clients.ToList());

    Task<Client?> IClientRepository.GetByIdAsync(uint id, CancellationToken cancellationToken) =>
        Task.FromResult(Clients.FirstOrDefault(c => c.Id == id));

    Task<IReadOnlyList<Professional>> IProfessionalRepository.ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Professional>>(Professionals.ToList());

    Task<Professional?> IProfessionalRepository.GetByIdAsync(uint id, CancellationToken cancellationToken) =>
        Task.FromResult(Professionals.FirstOrDefault(p => p.Id == id));

    Task<IReadOnlyList<Institution>> IInstitutionRepository.ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Institution>>(Institutions.ToList());

    Task<Institution?> IInstitutionRepository.GetByIdAsync(uint id, CancellationToken cancellationToken) =>
        Task.FromResult(Institutions.FirstOrDefault(i => i.Id == id));

    Task<IReadOnlyList<Medication>> IMedicationRepository.SearchActiveAsync(string? text, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Medication>>(
            Medications.Where(m => m.IsActive)
                       .Where(m => string.IsNullOrEmpty(text)
                                   || m.Name.Contains(text, StringComparison.OrdinalIgnoreCase)
                                   || m.ActiveIngredient.Contains(text, StringComparison.OrdinalIgnoreCase))
                       .ToList());

    Task<Medication?> IMedicationRepository.GetByIdAsync(uint id, CancellationToken cancellationToken) =>
        Task.FromResult(Medications.FirstOrDefault(m => m.Id == id));

    Task<IReadOnlyList<Medication>> IMedicationRepository.GetByIdsAsync(IEnumerable<uint> ids, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Medication>>(Medications.Where(m => ids.Contains(m.Id)).ToList());

    // ---- Lookups ----

    Task<IReadOnlyList<PharmaceuticalForm>> ILookupRepository.ListPharmaceuticalFormsAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<PharmaceuticalForm>>(PharmaceuticalForms.ToList());

    Task<IReadOnlyList<AdministrationRoute>> ILookupRepository.ListAdministrationRoutesAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<AdministrationRoute>>(AdministrationRoutes.ToList());

    Task<IReadOnlyList<DoseUnit>> ILookupRepository.ListDoseUnitsAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<DoseUnit>>(DoseUnits.ToList());

    Task<IReadOnlyList<Frequency>> ILookupRepository.ListFrequenciesAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Frequency>>(Frequencies.ToList());

    Task<IReadOnlyList<ValidityPeriod>> ILookupRepository.ListValidityPeriodsAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ValidityPeriod>>(ValidityPeriods.ToList());

    Task<bool> ILookupRepository.PharmaceuticalFormExistsAsync(uint id, CancellationToken cancellationToken) =>
        Task.FromResult(PharmaceuticalForms.Any(x => x.Id == id));

    Task<bool> ILookupRepository.AdministrationRouteExistsAsync(uint id, CancellationToken cancellationToken) =>
        Task.FromResult(AdministrationRoutes.Any(x => x.Id == id));

    Task<bool> ILookupRepository.DoseUnitExistsAsync(uint id, CancellationToken cancellationToken) =>
        Task.FromResult(DoseUnits.Any(x => x.Id == id));

    Task<bool> ILookupRepository.FrequencyExistsAsync(uint id, CancellationToken cancellationToken) =>
        Task.FromResult(Frequencies.Any(x => x.Id == id));
}
