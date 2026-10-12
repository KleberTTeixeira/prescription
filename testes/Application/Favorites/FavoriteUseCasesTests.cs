using Prescription.Application.Exceptions;
using Prescription.Application.Favorites;
using Prescription.Domain.Entities;
using Prescription.Tests.Application.Fakes;
using static Prescription.Tests.Application.Fakes.TestContext;

namespace Prescription.Tests.Application.Favorites;

public class FavoriteUseCasesTests
{
    private readonly TestContext _ctx = new();

    private AddFavorite Add() => new(_ctx.Db, _ctx.Db, _ctx.Db, _ctx.Db, _ctx.Clock);

    private RemoveFavorite Remove() => new(_ctx.Db, _ctx.Db);

    [Fact]
    public async Task Add_SavesFavoriteWithCreatedAt()
    {
        await Add().ExecuteAsync(Ana, Amoxicillin);

        var favorite = Assert.Single(_ctx.Db.Favorites);
        Assert.Equal(Ana, favorite.ProfessionalId);
        Assert.Equal(Amoxicillin, favorite.MedicationId);
        Assert.Equal(Now, favorite.CreatedAt);
    }

    [Fact]
    public async Task Add_AlreadyFavorite_IsIgnored()
    {
        await Add().ExecuteAsync(Ana, Amoxicillin);

        await Add().ExecuteAsync(Ana, Amoxicillin);

        Assert.Single(_ctx.Db.Favorites);
        Assert.Equal(1, _ctx.Db.SaveCount);
    }

    [Fact]
    public async Task Add_InactiveMedication_Throws()
    {
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => Add().ExecuteAsync(Ana, Inactive));
        Assert.Equal("Medicamento 'Medicamento Antigo 10 mg' está inativo.", ex.Message);
    }

    [Fact]
    public async Task Add_MedicationNotFound_Throws404()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => Add().ExecuteAsync(Ana, 99));
        Assert.Equal("Medicamento 99 não encontrado.", ex.Message);
    }

    [Fact]
    public async Task Add_ProfessionalNotFound_Throws()
    {
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => Add().ExecuteAsync(99, Amoxicillin));
        Assert.Equal("Profissional 99 não encontrado.", ex.Message);
    }

    [Fact]
    public async Task List_ReturnsOwnActiveFavoritesOnly()
    {
        await Add().ExecuteAsync(Ana, Amoxicillin);
        await Add().ExecuteAsync(Ana, Dipyrone);
        await Add().ExecuteAsync(Carlos, Clonazepam);
        _ctx.Db.Favorites.Add(new FavoriteMedication(Ana, Inactive, Now));

        var list = await new ListFavorites(_ctx.Db).ExecuteAsync(Ana);

        Assert.Equal([Dipyrone, Amoxicillin], list.Select(m => m.Id).Order());
        var amoxicillin = list.Single(m => m.Id == Amoxicillin);
        Assert.Equal("Amoxicilina 500 mg", amoxicillin.DisplayName);
        Assert.Equal(10u, amoxicillin.ValidityPeriodDays);
        Assert.True(amoxicillin.IsAntimicrobial);
    }

    [Fact]
    public async Task Remove_DeletesFavorite()
    {
        await Add().ExecuteAsync(Ana, Amoxicillin);

        await Remove().ExecuteAsync(Ana, Amoxicillin);

        Assert.Empty(_ctx.Db.Favorites);
    }

    [Fact]
    public async Task Remove_NotFavorite_IsIgnored()
    {
        await Remove().ExecuteAsync(Ana, Amoxicillin);

        Assert.Equal(0, _ctx.Db.SaveCount);
    }
}
