using Prescription.Application.Exceptions;
using Prescription.Application.Templates;
using Prescription.Domain.Exceptions;
using Prescription.Tests.Application.Fakes;
using static Prescription.Tests.Application.Fakes.TestContext;

namespace Prescription.Tests.Application.Templates;

public class TemplateUseCasesTests
{
    private readonly TestContext _ctx = new();

    private Task<TemplateDto> CreateAsync(uint professionalId, string title, string content = "Conteúdo") =>
        new CreateTemplate(_ctx.Db, _ctx.Db, _ctx.Db, _ctx.Clock)
            .ExecuteAsync(professionalId, new TemplateRequest(title, content));

    private UpdateTemplate Update() => new(_ctx.Db, _ctx.Db, _ctx.Clock);

    [Fact]
    public async Task Create_SavesWithCreatedAtNow()
    {
        var dto = await CreateAsync(Ana, "Analgesia simples", "Dipirona 500 mg");

        Assert.NotEqual(0u, dto.Id);
        Assert.Equal("Analgesia simples", dto.Title);
        Assert.Equal("Dipirona 500 mg", dto.Content);
        Assert.Equal(Now, dto.CreatedAt);
        Assert.Null(dto.UpdatedAt);
        Assert.Equal(Ana, _ctx.Db.Templates.Single().ProfessionalId);
    }

    [Fact]
    public async Task Create_DuplicatedTitleForSameProfessional_Throws()
    {
        await CreateAsync(Ana, "Hidratação");

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => CreateAsync(Ana, "Hidratação"));
        Assert.Equal("Já existe texto padrão com o título 'Hidratação'.", ex.Message);
    }

    [Fact]
    public async Task Create_SameTitleForOtherProfessional_IsAllowed()
    {
        await CreateAsync(Ana, "Hidratação");
        await CreateAsync(Carlos, "Hidratação");

        Assert.Equal(2, _ctx.Db.Templates.Count);
    }

    [Fact]
    public async Task Create_ProfessionalNotFound_Throws()
    {
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => CreateAsync(99, "Título"));
        Assert.Equal("Profissional 99 não encontrado.", ex.Message);
    }

    [Fact]
    public async Task Create_WithoutContent_IsRejectedByDomain()
    {
        var ex = await Assert.ThrowsAsync<DomainException>(() => CreateAsync(Ana, "Título", " "));
        Assert.Equal("Campo 'Conteúdo do texto padrão' é obrigatório.", ex.Message);
    }

    [Fact]
    public async Task List_ReturnsOnlyOwnTemplates()
    {
        await CreateAsync(Ana, "B");
        await CreateAsync(Ana, "A");
        await CreateAsync(Carlos, "C");

        var list = await new ListTemplates(_ctx.Db).ExecuteAsync(Ana);

        Assert.Equal(["A", "B"], list.Select(t => t.Title));
    }

    [Fact]
    public async Task Get_OwnTemplate_IsReturned()
    {
        var created = await CreateAsync(Ana, "Base");

        var dto = await new GetTemplate(_ctx.Db).ExecuteAsync(Ana, created.Id);

        Assert.Equal("Base", dto.Title);
    }

    [Fact]
    public async Task Get_TemplateOfOtherProfessional_Throws()
    {
        var created = await CreateAsync(Ana, "Base");

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            new GetTemplate(_ctx.Db).ExecuteAsync(Carlos, created.Id));

        Assert.Equal("Texto padrão pertence a outro profissional.", ex.Message);
    }

    [Fact]
    public async Task Get_NotFound_Throws404()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => new GetTemplate(_ctx.Db).ExecuteAsync(Ana, 99));
        Assert.Equal("Texto padrão 99 não encontrado.", ex.Message);
    }

    [Fact]
    public async Task Update_ChangesTextAndSetsUpdatedAt()
    {
        var created = await CreateAsync(Ana, "Título");
        _ctx.Clock.Now = Now.AddHours(1);

        var dto = await Update().ExecuteAsync(Ana, created.Id, new TemplateRequest("Título", "Novo conteúdo"));

        Assert.Equal("Novo conteúdo", dto.Content);
        Assert.Equal(Now.AddHours(1), dto.UpdatedAt);
        Assert.Equal(Now, dto.CreatedAt);
    }

    [Fact]
    public async Task Update_ToTitleOfAnotherOwnTemplate_Throws()
    {
        await CreateAsync(Ana, "Primeiro");
        var second = await CreateAsync(Ana, "Segundo");

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Update().ExecuteAsync(Ana, second.Id, new TemplateRequest("Primeiro", "x")));

        Assert.Equal("Já existe texto padrão com o título 'Primeiro'.", ex.Message);
    }

    [Fact]
    public async Task Update_TemplateOfOtherProfessional_Throws()
    {
        var created = await CreateAsync(Ana, "Título");

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Update().ExecuteAsync(Carlos, created.Id, new TemplateRequest("Título", "x")));

        Assert.Equal("Texto padrão pertence a outro profissional.", ex.Message);
    }

    [Fact]
    public async Task Delete_OwnTemplate_Removes()
    {
        var created = await CreateAsync(Ana, "Título");

        await new DeleteTemplate(_ctx.Db, _ctx.Db).ExecuteAsync(Ana, created.Id);

        Assert.Empty(_ctx.Db.Templates);
    }

    [Fact]
    public async Task Delete_TemplateOfOtherProfessional_Throws()
    {
        var created = await CreateAsync(Ana, "Título");

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            new DeleteTemplate(_ctx.Db, _ctx.Db).ExecuteAsync(Carlos, created.Id));

        Assert.Single(_ctx.Db.Templates);
    }
}
