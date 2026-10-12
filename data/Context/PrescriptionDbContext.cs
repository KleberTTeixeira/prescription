using Microsoft.EntityFrameworkCore;
using Prescription.Domain.Entities;

namespace Prescription.Data.Context;

/// <summary>Maps the existing tables of prescription_db (created by the SQL scripts; no migrations).</summary>
public sealed class PrescriptionDbContext(DbContextOptions<PrescriptionDbContext> options) : DbContext(options)
{
    public DbSet<PharmaceuticalForm> PharmaceuticalForms => Set<PharmaceuticalForm>();
    public DbSet<AdministrationRoute> AdministrationRoutes => Set<AdministrationRoute>();
    public DbSet<DoseUnit> DoseUnits => Set<DoseUnit>();
    public DbSet<Frequency> Frequencies => Set<Frequency>();
    public DbSet<ValidityPeriod> ValidityPeriods => Set<ValidityPeriod>();
    public DbSet<Institution> Institutions => Set<Institution>();
    public DbSet<Professional> Professionals => Set<Professional>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<FavoriteMedication> FavoriteMedications => Set<FavoriteMedication>();
    public DbSet<PrescriptionTemplate> PrescriptionTemplates => Set<PrescriptionTemplate>();
    public DbSet<MedicalPrescription> Prescriptions => Set<MedicalPrescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PrescriptionDbContext).Assembly);
}
