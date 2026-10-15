using System.Globalization;
using MudBlazor.Services;
using Prescription.Blazor;
using Prescription.Blazor.Components;
using Prescription.Blazor.Services;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = AppCulture.PtBr;

var builder = WebApplication.CreateBuilder(args);

var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("Configuração 'ApiBaseUrl' não informada.");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomCenter;
    config.SnackbarConfiguration.ShowCloseIcon = true;
});

builder.Services.AddScoped<ILoggedProfessionalStore, LoggedProfessionalStore>();
builder.Services.AddSingleton<IClock, BrasiliaClock>();

void ConfigureApi(HttpClient client) => client.BaseAddress = new Uri(apiBaseUrl);
builder.Services.AddHttpClient<CatalogApiClient>(ConfigureApi);
builder.Services.AddHttpClient<PrescriptionApiClient>(ConfigureApi);
builder.Services.AddHttpClient<MedicationApiClient>(ConfigureApi);
builder.Services.AddHttpClient<TemplateApiClient>(ConfigureApi);
builder.Services.AddHttpClient<FavoriteApiClient>(ConfigureApi);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
