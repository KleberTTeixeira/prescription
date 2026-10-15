using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Prescription.Blazor.Services;
using Prescription.Blazor.Tests.Fakes;

namespace Prescription.Blazor.Tests;

/// <summary>bUnit context with MudBlazor, a fake API (HTTP handler) and a fake professional store.</summary>
public abstract class BlazorTestContext : BunitContext, IAsyncLifetime
{
    // MudBlazor services are only IAsyncDisposable: xUnit must dispose the context asynchronously.
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;
    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    protected BlazorTestContext()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
    }

    protected FakeLoggedProfessionalStore Store { get; private set; } = new();

    protected void UseStore(FakeLoggedProfessionalStore store)
    {
        Store = store;
        Services.AddSingleton<ILoggedProfessionalStore>(store);
    }

    protected void UseApi(FakeHttpMessageHandler handler) =>
        Services.AddSingleton(provider => new CatalogApiClient(handler.CreateClient(),
                                                               provider.GetRequiredService<ILoggedProfessionalStore>()));

    protected void UsePrescriptionApi(FakeHttpMessageHandler handler) =>
        Services.AddSingleton(provider => new PrescriptionApiClient(handler.CreateClient(),
                                                                    provider.GetRequiredService<ILoggedProfessionalStore>()));

    /// <summary>All the API clients answered by <paramref name="api"/>; today (Brasília) = 15/10/2026.</summary>
    protected void UseAllApis(FakeApi api)
    {
        T Create<T>(IServiceProvider provider, Func<HttpClient, ILoggedProfessionalStore, T> create) =>
            create(api.Handler.CreateClient(), provider.GetRequiredService<ILoggedProfessionalStore>());

        Services.AddSingleton(provider => Create(provider, (http, store) => new CatalogApiClient(http, store)));
        Services.AddSingleton(provider => Create(provider, (http, store) => new PrescriptionApiClient(http, store)));
        Services.AddSingleton(provider => Create(provider, (http, store) => new MedicationApiClient(http, store)));
        Services.AddSingleton(provider => Create(provider, (http, store) => new TemplateApiClient(http, store)));
        Services.AddSingleton(provider => Create(provider, (http, store) => new FavoriteApiClient(http, store)));
        Services.AddSingleton<IClock>(new FakeClock(Today));
    }

    protected static readonly DateOnly Today = new(2026, 10, 15);

    protected string CurrentPath => new Uri(Services.GetRequiredService<NavigationManager>().Uri).AbsolutePath;

    /// <summary>Same as choosing an option of the MudSelect marked with data-testid (opening the popup is JavaScript).</summary>
    protected static void Select<T>(IRenderedComponent<IComponent> page, string testId, T value, int index = 0)
    {
        var select = page.FindComponents<MudBlazor.MudSelect<T>>().Where(s => HasTestId(s.Instance, testId)).ElementAt(index);
        select.InvokeAsync(() => select.Instance.ValueChanged.InvokeAsync(value)).GetAwaiter().GetResult();
    }

    /// <summary>Same as clicking an option of the MudToggleGroup marked with data-testid.</summary>
    protected static void Choose<T>(IRenderedComponent<IComponent> page, string testId, T value)
    {
        var group = page.FindComponents<MudBlazor.MudToggleGroup<T>>().First(g => HasTestId(g.Instance, testId));
        group.InvokeAsync(() => group.Instance.ValueChanged.InvokeAsync(value)).GetAwaiter().GetResult();
    }

    /// <summary>Same as typing in the MudTextField marked with data-testid (after its debounce).</summary>
    protected static void Type(IRenderedComponent<IComponent> page, string testId, string text)
    {
        var field = page.FindComponents<MudBlazor.MudTextField<string>>().First(f => HasTestId(f.Instance, testId));
        field.InvokeAsync(() => field.Instance.ValueChanged.InvokeAsync(text)).GetAwaiter().GetResult();
    }

    /// <summary>What the medication autocomplete lists for <paramref name="term"/> (the list itself opens in a popover).</summary>
    protected static IReadOnlyList<Prescription.Blazor.Models.Medication> SearchMedication(IRenderedComponent<IComponent> page, string term, int index = 0)
    {
        var search = page.FindComponents<MudBlazor.MudAutocomplete<Prescription.Blazor.Models.Medication>>()[index];
        var searchFunc = search.Instance.SearchFunc ?? throw new InvalidOperationException("Autocompletar sem busca.");
        var found = search.InvokeAsync(async () =>
        {
            var task = searchFunc(term, CancellationToken.None);
            return task is null ? null : await task;
        }).GetAwaiter().GetResult();
        return found?.ToList() ?? [];
    }

    /// <summary>Same as choosing <paramref name="medication"/> in the medication autocomplete.</summary>
    protected static void ChooseMedication(IRenderedComponent<IComponent> page, Prescription.Blazor.Models.Medication medication, int index = 0)
    {
        var search = page.FindComponents<MudBlazor.MudAutocomplete<Prescription.Blazor.Models.Medication>>()[index];
        search.InvokeAsync(() => search.Instance.ValueChanged.InvokeAsync(medication)).GetAwaiter().GetResult();
    }

    /// <summary>Messages shown in the floating notices (snackbars).</summary>
    protected IReadOnlyList<string> Notices =>
        Services.GetRequiredService<MudBlazor.ISnackbar>().ShownSnackbars.Select(s => s.Message ?? "").ToList();

    private static bool HasTestId(MudBlazor.MudComponentBase component, string testId) =>
        component.UserAttributes.TryGetValue("data-testid", out var id) && (string?)id == testId;
}
