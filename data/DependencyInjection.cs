using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prescription.Application.Abstractions;
using Prescription.Data.Context;
using Prescription.Data.Repositories;
using Prescription.Data.Services;

namespace Prescription.Data;

public static class DependencyInjection
{
    /// <summary>Registers the DbContext (MySQL), repositories, unit of work and clock.</summary>
    public static IServiceCollection AddData(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<PrescriptionDbContext>(options => options.UseMySQL(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
        services.AddScoped<IPrescriptionTemplateRepository, PrescriptionTemplateRepository>();
        services.AddScoped<IFavoriteMedicationRepository, FavoriteMedicationRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IProfessionalRepository, ProfessionalRepository>();
        services.AddScoped<IInstitutionRepository, InstitutionRepository>();
        services.AddScoped<IMedicationRepository, MedicationRepository>();
        services.AddScoped<ILookupRepository, LookupRepository>();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IClock, BrasiliaClock>();

        return services;
    }
}
