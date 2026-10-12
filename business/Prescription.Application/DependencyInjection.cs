using Microsoft.Extensions.DependencyInjection;
using Prescription.Application.Catalog;
using Prescription.Application.Favorites;
using Prescription.Application.Prescriptions;
using Prescription.Application.Templates;

namespace Prescription.Application;

public static class DependencyInjection
{
    /// <summary>Registers the use cases. Repositories, unit of work and clock are registered by Prescription.Data.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Prescriptions
        services.AddScoped<PrescriptionInputResolver>();
        services.AddScoped<CreatePrescription>();
        services.AddScoped<UpdatePrescription>();
        services.AddScoped<DeletePrescription>();
        services.AddScoped<GetPrescription>();
        services.AddScoped<ListPrescriptions>();
        services.AddScoped<MarkPrescriptionAsPrinted>();
        services.AddScoped<GetPrescriptionPrintData>();
        services.AddScoped<SuggestPrescriptionCategory>();
        services.AddScoped<SuggestUsageDeadline>();

        // Standard texts (ECF.10.02)
        services.AddScoped<ListTemplates>();
        services.AddScoped<GetTemplate>();
        services.AddScoped<CreateTemplate>();
        services.AddScoped<UpdateTemplate>();
        services.AddScoped<DeleteTemplate>();

        // Favorites (ECF.10.10)
        services.AddScoped<ListFavorites>();
        services.AddScoped<AddFavorite>();
        services.AddScoped<RemoveFavorite>();

        // Catalog
        services.AddScoped<ListClients>();
        services.AddScoped<ListProfessionals>();
        services.AddScoped<ListInstitutions>();
        services.AddScoped<SearchMedications>();
        services.AddScoped<ListPharmaceuticalForms>();
        services.AddScoped<ListAdministrationRoutes>();
        services.AddScoped<ListDoseUnits>();
        services.AddScoped<ListFrequencies>();
        services.AddScoped<ListValidityPeriods>();

        return services;
    }
}
