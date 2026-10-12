using Microsoft.EntityFrameworkCore;
using Prescription.Application.Exceptions;
using Prescription.Application.Prescriptions;
using Prescription.Domain.Enums;
using Prescription.Domain.Exceptions;

namespace Prescription.IntegrationTests;

/// <summary>Prescription use cases against MySQL (02_seed.sql: prescriptions 1 to 4).</summary>
public class PrescriptionTests : DatabaseTest
{
    private static PrescriptionItemRequest CatalogItem(uint medicationId) =>
        new(medicationId, null, null, 1.5m, 6, 3, new DateOnly(2026, 10, 11), 7, false, 1,
            new DateOnly(2026, 10, 21), "Após as refeições");

    private static PrescriptionItemRequest CompoundedItem() =>
        new(null, "Cápsula manipulada de melatonina 3 mg", 2, 1m, 7, 1, new DateOnly(2026, 10, 11), null, true, 1,
            null, null);

    [SkippableFact]
    public async Task GetSeededPrescription_MapsAllColumns()
    {
        RequireDatabase();

        var p = await Get<GetPrescription>().ExecuteAsync(2);

        Assert.Equal("Maria Clara Oliveira", p.ClientName);
        Assert.Equal("Carlos Eduardo Lima", p.ProfessionalName);
        Assert.Equal("Hospital Santa Clara", p.InstitutionName);
        Assert.Equal(PrescriptionFormat.Structured, p.Format);
        Assert.Equal(PrescriptionCategory.Antimicrobial, p.Category);
        Assert.Null(p.FreeText);
        Assert.Equal(new DateTime(2026, 10, 2, 14, 10, 0), p.IssuedAt);
        Assert.Equal(new DateTime(2026, 10, 2, 14, 25, 0), p.PrintedAt);
        Assert.True(p.IsIssued);

        var item = Assert.Single(p.Items);
        Assert.Equal(7u, item.MedicationId);
        Assert.Equal("Azitromicina 500 mg", item.MedicationName);
        Assert.Equal("Comprimido", item.PharmaceuticalFormName);
        Assert.Equal(1m, item.Dose);
        Assert.Equal("comp", item.DoseUnitAbbreviation);
        Assert.Equal(new DateOnly(2026, 10, 2), item.StartDate);
        Assert.Equal(5u, item.DurationDays);
        Assert.False(item.IsContinuousUse);
        Assert.Equal(new DateOnly(2026, 10, 12), item.UsageDeadline);
        Assert.Equal("Tomar 1 hora antes ou 2 horas após as refeições.", item.Notes);
    }

    [SkippableFact]
    public async Task GetSeededPrescription_WithCompoundedItem()
    {
        RequireDatabase();

        var p = await Get<GetPrescription>().ExecuteAsync(3);

        Assert.Equal(3, p.Items.Count);
        var compounded = p.Items.Single(i => i.MedicationId is null);
        Assert.Equal("Cápsula manipulada de vitamina D3 7.000 UI", compounded.MedicationName);
        Assert.Null(compounded.UsageDeadline);
        Assert.False(p.IsIssued);
    }

    [SkippableFact]
    public async Task List_IsTranslatedToSql_OnlyOwnPrescriptions_MostRecentFirst()
    {
        RequireDatabase();

        var rows = await Get<ListPrescriptions>().Execute(2).ToListAsync();

        Assert.Equal([4u, 2u], rows.Select(r => r.Id));
        var row = rows[0];
        Assert.Equal("Rafael Almeida Rocha", row.ClientName);
        Assert.Equal(PrescriptionCategory.SpecialControl, row.Category);
        Assert.False(row.IsIssued);
        Assert.Equal(1, row.ItemCount);
        Assert.True(rows[1].IsIssued);
    }

    [SkippableFact]
    public async Task List_AcceptsFilterOrderAndProjection_InSql()
    {
        RequireDatabase();

        var ids = await Get<ListPrescriptions>().Execute(2)
                                                .Where(r => r.Category == PrescriptionCategory.Antimicrobial)
                                                .OrderBy(r => r.IssuedAt)
                                                .Select(r => r.Id)
                                                .ToListAsync();

        Assert.Equal([2u], ids);
    }

    [SkippableFact]
    public async Task Create_PersistsPrescriptionAndItems()
    {
        RequireDatabase();

        var request = new PrescriptionRequest(1, 1, PrescriptionFormat.Structured, PrescriptionCategory.Antimicrobial,
                                              "Retornar em 7 dias", [CatalogItem(5), CompoundedItem()]);

        var created = await Get<CreatePrescription>().ExecuteAsync(1, request);
        Context.ChangeTracker.Clear();
        var loaded = await Get<GetPrescription>().ExecuteAsync(created.Id);

        Assert.True(loaded.Id > 4);
        Assert.Equal("Retornar em 7 dias", loaded.FreeText);
        Assert.False(loaded.IsIssued);
        Assert.InRange(loaded.IssuedAt, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));
        Assert.Equal(2, loaded.Items.Count);
        var amoxicillin = loaded.Items.Single(i => i.MedicationId == 5);
        Assert.Equal("Amoxicilina 500 mg", amoxicillin.MedicationName);
        Assert.Equal(2u, amoxicillin.PharmaceuticalFormId);
        Assert.Equal(1.5m, amoxicillin.Dose);
        Assert.Equal(new DateOnly(2026, 10, 21), amoxicillin.UsageDeadline);
        Assert.Contains(loaded.Items, i => i.MedicationId is null && i.IsContinuousUse);

        var entity = await Context.Prescriptions.AsNoTracking().SingleAsync(p => p.Id == created.Id);
        Assert.Equal(entity.IssuedAt, entity.CreatedAt);
        Assert.Null(entity.UpdatedAt);
    }

    [SkippableFact]
    public async Task Update_ToUnstructured_DeletesItemsInDatabase()
    {
        RequireDatabase();

        var dto = await Get<UpdatePrescription>().ExecuteAsync(3, 3,
            new PrescriptionRequest(4, 3, PrescriptionFormat.Unstructured, PrescriptionCategory.Common,
                                    "Losartana 50 mg 1x ao dia", null));
        Context.ChangeTracker.Clear();

        Assert.Equal(PrescriptionFormat.Unstructured, dto.Format);
        Assert.Equal(0, await Context.PrescriptionItems.CountAsync(i => i.PrescriptionId == 3));
        var entity = await Context.Prescriptions.AsNoTracking().SingleAsync(p => p.Id == 3);
        Assert.Equal("Losartana 50 mg 1x ao dia", entity.FreeText);
        Assert.NotNull(entity.UpdatedAt);
    }

    [SkippableFact]
    public async Task MarkAsPrinted_PersistsAndLocksPrescription()
    {
        RequireDatabase();

        await Get<MarkPrescriptionAsPrinted>().ExecuteAsync(3, 3);
        Context.ChangeTracker.Clear();

        var entity = await Context.Prescriptions.AsNoTracking().SingleAsync(p => p.Id == 3);
        Assert.NotNull(entity.PrintedAt);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            Get<UpdatePrescription>().ExecuteAsync(3, 3,
                new PrescriptionRequest(4, 3, PrescriptionFormat.Unstructured, PrescriptionCategory.Common, "x", null)));
        Assert.Equal("Receita já emitida (impressa) não pode ser alterada.", ex.Message);
    }

    [SkippableFact]
    public async Task Update_SeededIssuedPrescription_IsRejected()
    {
        RequireDatabase();

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            Get<UpdatePrescription>().ExecuteAsync(1, 1,
                new PrescriptionRequest(1, 1, PrescriptionFormat.Unstructured, PrescriptionCategory.Common, "x", null)));

        Assert.Equal("Receita já emitida (impressa) não pode ser alterada.", ex.Message);
    }

    [SkippableFact]
    public async Task Delete_RemovesPrescriptionAndItems()
    {
        RequireDatabase();

        await Get<DeletePrescription>().ExecuteAsync(3, 3);
        Context.ChangeTracker.Clear();

        Assert.False(await Context.Prescriptions.AnyAsync(p => p.Id == 3));
        Assert.Equal(0, await Context.PrescriptionItems.CountAsync(i => i.PrescriptionId == 3));
        await Assert.ThrowsAsync<NotFoundException>(() => Get<GetPrescription>().ExecuteAsync(3));
    }

    [SkippableFact]
    public async Task PrintData_SpecialControl_HasClientAddress()
    {
        RequireDatabase();

        var print = await Get<GetPrescriptionPrintData>().ExecuteAsync(4);

        Assert.Equal("Rafael Almeida Rocha", print.Client.FullName);
        Assert.Equal("Avenida Paulista", print.Client.Address!.Street);
        Assert.Equal("Conj. 45", print.Client.Address.Complement);
        Assert.Null(print.Client.Age);
        Assert.Equal("Hospital Santa Clara", print.Institution.Name);
        Assert.Equal("Clonazepam 2 mg", Assert.Single(print.Items).MedicationName);
    }

    [SkippableFact]
    public async Task PrintData_Antimicrobial_HasAgeAndSex()
    {
        RequireDatabase();

        var print = await Get<GetPrescriptionPrintData>().ExecuteAsync(2);

        Assert.NotNull(print.Client.Age);
        Assert.Equal(Sex.Female, print.Client.Sex);
        Assert.Null(print.Client.Address);
    }

    [SkippableFact]
    public async Task Suggestions_UseCatalogData()
    {
        RequireDatabase();

        Assert.Equal(PrescriptionCategory.SpecialControl,
                     await Get<SuggestPrescriptionCategory>().ExecuteAsync([5, 14]));
        Assert.Equal(PrescriptionCategory.Antimicrobial,
                     await Get<SuggestPrescriptionCategory>().ExecuteAsync([1, 7]));
        Assert.Equal(new DateOnly(2026, 10, 15),
                     await Get<SuggestUsageDeadline>().ExecuteAsync(5, new DateOnly(2026, 10, 5)));
        Assert.Null(await Get<SuggestUsageDeadline>().ExecuteAsync(1, new DateOnly(2026, 10, 5)));
    }
}
