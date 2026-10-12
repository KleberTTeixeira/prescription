using Prescription.Application.Prescriptions;
using Prescription.Domain.Entities;
using Prescription.Domain.Enums;
using Prescription.Domain.ValueObjects;

namespace Prescription.Tests.Application.Fakes;

/// <summary>In-memory database with catalog data (similar to 02_seed.sql), fake clock and request builders.</summary>
internal sealed class TestContext
{
    public const uint Ana = 1;           // professional
    public const uint Carlos = 2;        // professional
    public const uint Joao = 1;          // client, M, born 1980-03-15
    public const uint Maria = 2;         // client, F, born 1992-10-12
    public const uint Clinic = 1;        // institution
    public const uint Tablet = 1;        // pharmaceutical form
    public const uint Capsule = 2;       // pharmaceutical form
    public const uint Oral = 1;          // administration route
    public const uint Mg = 1;            // dose unit
    public const uint OncePerDay = 1;    // frequency
    public const uint Dipyrone = 1;      // common, no validity limit
    public const uint Amoxicillin = 5;   // antimicrobial, 10 days
    public const uint Clonazepam = 14;   // controlled, 30 days
    public const uint Saline = 31;       // common, no concentration
    public const uint Inactive = 30;     // inactive

    public static readonly DateTime Now = new(2026, 10, 11, 10, 30, 0);

    public InMemoryDatabase Db { get; } = new();
    public FakeClock Clock { get; } = new(Now);

    public TestContext()
    {
        var tablet = new PharmaceuticalForm("Comprimido").WithId(Tablet);
        var capsule = new PharmaceuticalForm("Cápsula").WithId(Capsule);
        Db.PharmaceuticalForms.AddRange([tablet, capsule]);
        Db.AdministrationRoutes.Add(new AdministrationRoute("Oral").WithId(Oral));
        Db.DoseUnits.Add(new DoseUnit("mg", "Miligrama").WithId(Mg));
        Db.Frequencies.Add(new Frequency("1 vez ao dia").WithId(OncePerDay));

        var noLimit = new ValidityPeriod("Sem prazo", null).WithId(1);
        var antimicrobial = new ValidityPeriod("Antimicrobiano - 10 dias", 10).WithId(2);
        var control = new ValidityPeriod("Controle especial - 30 dias", 30).WithId(4);
        Db.ValidityPeriods.AddRange([noLimit, antimicrobial, control]);

        Db.Professionals.Add(new Professional("Ana Beatriz Moreira", "CRM", "123456", "SP").WithId(Ana));
        Db.Professionals.Add(new Professional("Carlos Eduardo Lima", "CRM", "234567", "SP").WithId(Carlos));

        Db.Institutions.Add(new Institution("Clínica Vida Plena", "1234567",
            new Address("Rua das Acácias", "120", null, "Jardim Paulista", "São Paulo", "SP", "01415000"),
            "(11) 3000-1001").WithId(Clinic));

        Db.Clients.Add(new Client("João Pedro Silva", new DateOnly(1980, 3, 15), Sex.Male,
            new Address("Rua Augusta", "500", "Apto 12", "Consolação", "São Paulo", "SP", "01304000")).WithId(Joao));
        Db.Clients.Add(new Client("Maria Clara Oliveira", new DateOnly(1992, 10, 12), Sex.Female,
            new Address("Rua Barão de Jaguara", "210", null, "Centro", "Campinas", "SP", "13015000")).WithId(Maria));

        AddMedication(Dipyrone, "Dipirona Sódica", "500 mg", tablet, noLimit, controlled: false, antimicrobialFlag: false);
        AddMedication(Amoxicillin, "Amoxicilina", "500 mg", capsule, antimicrobial, controlled: false, antimicrobialFlag: true);
        AddMedication(Clonazepam, "Clonazepam", "2 mg", tablet, control, controlled: true, antimicrobialFlag: false);
        AddMedication(Saline, "Soro Fisiológico", null, tablet, noLimit, controlled: false, antimicrobialFlag: false);
        AddMedication(Inactive, "Medicamento Antigo", "10 mg", tablet, noLimit, controlled: false, antimicrobialFlag: false,
                      active: false);
    }

    private void AddMedication(uint id, string name, string? concentration, PharmaceuticalForm form,
                               ValidityPeriod validity, bool controlled, bool antimicrobialFlag, bool active = true)
    {
        Db.Medications.Add(new Medication(name, name, concentration, form.Id, validity.Id, controlled,
                                          antimicrobialFlag, active)
            .WithId(id)
            .With("PharmaceuticalForm", form)
            .With("ValidityPeriod", validity));
    }

    // ---- Request builders ----

    public static PrescriptionItemRequest CatalogItem(uint medicationId, string? name = null, uint? formId = null) =>
        new(medicationId, name, formId, 1m, Mg, OncePerDay, new DateOnly(2026, 10, 11), 7, false, Oral,
            new DateOnly(2026, 10, 21), "Tomar após as refeições");

    public static PrescriptionItemRequest FreeItem(string name = "Fórmula manipulada", uint formId = Capsule) =>
        new(null, name, formId, 2m, Mg, OncePerDay, new DateOnly(2026, 10, 11), null, true, Oral, null, null);

    public static PrescriptionRequest Unstructured(string? freeText = "Dipirona 500 mg de 6/6h",
                                                   uint clientId = Joao, uint institutionId = Clinic,
                                                   PrescriptionCategory category = PrescriptionCategory.Common) =>
        new(clientId, institutionId, PrescriptionFormat.Unstructured, category, freeText, null);

    public static PrescriptionRequest Structured(params PrescriptionItemRequest[] items) =>
        new(Joao, Clinic, PrescriptionFormat.Structured, PrescriptionCategory.Common, null, items);

    public static PrescriptionRequest Structured(PrescriptionCategory category, uint clientId,
                                                 params PrescriptionItemRequest[] items) =>
        new(clientId, Clinic, PrescriptionFormat.Structured, category, null, items);

    // ---- Use cases ----

    public PrescriptionInputResolver Resolver() => new(Db, Db, Db, Db, Db);

    public async Task<PrescriptionDto> CreateAsync(PrescriptionRequest request, uint professionalId = Ana) =>
        await new CreatePrescription(Db, Resolver(), Db, Clock).ExecuteAsync(professionalId, request);
}
