using Microsoft.EntityFrameworkCore;
using Prescription.Application.Exceptions;
using Prescription.Application.Favorites;
using Prescription.Application.Templates;

namespace Prescription.IntegrationTests;

/// <summary>Standard texts (ECF.10.02) and favorites (ECF.10.10) against MySQL.</summary>
public class TemplateAndFavoriteTests : DatabaseTest
{
    [SkippableFact]
    public async Task Templates_ListedByTitle_ForTheProfessional()
    {
        RequireDatabase();

        var list = await Get<ListTemplates>().ExecuteAsync(1);

        Assert.Equal(["Analgesia simples", "Orientações de hidratação"], list.Select(t => t.Title));
    }

    [SkippableFact]
    public async Task Templates_CreateUpdateDelete()
    {
        RequireDatabase();

        var created = await Get<CreateTemplate>().ExecuteAsync(1, new TemplateRequest("Repouso", "Repouso por 2 dias."));
        var updated = await Get<UpdateTemplate>().ExecuteAsync(1, created.Id,
                                                              new TemplateRequest("Repouso", "Repouso por 3 dias."));
        Context.ChangeTracker.Clear();

        Assert.Equal("Repouso por 3 dias.", (await Get<GetTemplate>().ExecuteAsync(1, created.Id)).Content);
        Assert.NotNull(updated.UpdatedAt);

        await Get<DeleteTemplate>().ExecuteAsync(1, created.Id);
        Context.ChangeTracker.Clear();

        Assert.False(await Context.PrescriptionTemplates.AnyAsync(t => t.Id == created.Id));
    }

    [SkippableFact]
    public async Task Templates_DuplicatedTitle_IsCaseInsensitive()
    {
        RequireDatabase();

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Get<CreateTemplate>().ExecuteAsync(1, new TemplateRequest("analgesia simples", "x")));

        Assert.Equal("Já existe texto padrão com o título 'analgesia simples'.", ex.Message);
    }

    [SkippableFact]
    public async Task Favorites_ListedByMedicationName()
    {
        RequireDatabase();

        var list = await Get<ListFavorites>().ExecuteAsync(1);

        Assert.Equal([5u, 1u, 9u], list.Select(m => m.Id));
    }

    [SkippableFact]
    public async Task Favorites_AddIsIdempotent_AndRemove()
    {
        RequireDatabase();

        await Get<AddFavorite>().ExecuteAsync(1, 3);
        await Get<AddFavorite>().ExecuteAsync(1, 3);
        Context.ChangeTracker.Clear();

        Assert.Equal(1, await Context.FavoriteMedications.CountAsync(f => f.ProfessionalId == 1 && f.MedicationId == 3));

        await Get<RemoveFavorite>().ExecuteAsync(1, 3);
        await Get<RemoveFavorite>().ExecuteAsync(1, 3);
        Context.ChangeTracker.Clear();

        Assert.False(await Context.FavoriteMedications.AnyAsync(f => f.ProfessionalId == 1 && f.MedicationId == 3));
    }
}
