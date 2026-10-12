using Prescription.Application.Prescriptions;
using Prescription.Domain.Enums;
using Prescription.Tests.Application.Fakes;
using static Prescription.Tests.Application.Fakes.TestContext;

namespace Prescription.Tests.Application.Prescriptions;

public class ListPrescriptionsTests
{
    private readonly TestContext _ctx = new();

    [Fact]
    public async Task List_ReturnsOnlyPrescriptionsOfTheLoggedProfessional()
    {
        await _ctx.CreateAsync(Unstructured(), Ana);
        await _ctx.CreateAsync(Unstructured(), Ana);
        await _ctx.CreateAsync(Unstructured(), Carlos);

        var list = new ListPrescriptions(_ctx.Db).Execute(Ana).ToList();

        Assert.Equal(2, list.Count);
        Assert.All(list, row => Assert.Equal(Ana, row.ProfessionalId));
    }

    [Fact]
    public async Task List_ProjectsRowFields()
    {
        var created = await _ctx.CreateAsync(
            Structured(PrescriptionCategory.Antimicrobial, Maria, CatalogItem(Amoxicillin), FreeItem()));
        _ctx.Db.Prescriptions.Single().MarkAsPrinted(Now.AddMinutes(5));

        var row = Assert.Single(new ListPrescriptions(_ctx.Db).Execute(Ana));

        Assert.Equal(created.Id, row.Id);
        Assert.Equal(Maria, row.ClientId);
        Assert.Equal("Maria Clara Oliveira", row.ClientName);
        Assert.Equal("Ana Beatriz Moreira", row.ProfessionalName);
        Assert.Equal(Clinic, row.InstitutionId);
        Assert.Equal("Clínica Vida Plena", row.InstitutionName);
        Assert.Equal(PrescriptionFormat.Structured, row.Format);
        Assert.Equal(PrescriptionCategory.Antimicrobial, row.Category);
        Assert.Equal(Now, row.IssuedAt);
        Assert.Equal(Now.AddMinutes(5), row.PrintedAt);
        Assert.True(row.IsIssued);
        Assert.Equal(2, row.ItemCount);
    }

    [Fact]
    public void List_IsAQueryThatAcceptsFilterAndOrder()
    {
        IQueryable<PrescriptionListItemDto> query = new ListPrescriptions(_ctx.Db).Execute(Ana);

        Assert.Empty(query.Where(r => r.IsIssued).OrderBy(r => r.IssuedAt));
    }
}
