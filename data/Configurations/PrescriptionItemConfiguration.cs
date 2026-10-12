using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prescription.Data.Converters;
using Prescription.Domain.Entities;

namespace Prescription.Data.Configurations;

internal sealed class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.ToTable("prescription_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.PrescriptionId).HasColumnName("prescription_id");
        builder.Property(x => x.MedicationId).HasColumnName("medication_id");
        builder.Property(x => x.MedicationName).HasColumnName("medication_name").HasMaxLength(150).IsRequired();
        builder.Property(x => x.PharmaceuticalFormId).HasColumnName("pharmaceutical_form_id");
        builder.Property(x => x.Dose).HasColumnName("dose").HasPrecision(10, 3);
        builder.Property(x => x.DoseUnitId).HasColumnName("dose_unit_id");
        builder.Property(x => x.FrequencyId).HasColumnName("frequency_id");
        builder.Property(x => x.StartDate).HasColumnName("start_date").HasColumnType("date")
               .HasConversion(DateOnlyConverters.Required);
        builder.Property(x => x.DurationDays).HasColumnName("duration_days");
        builder.Property(x => x.IsContinuousUse).HasColumnName("is_continuous_use");
        builder.Property(x => x.AdministrationRouteId).HasColumnName("administration_route_id");
        builder.Property(x => x.UsageDeadline).HasColumnName("usage_deadline").HasColumnType("date")
               .HasConversion(DateOnlyConverters.Optional);
        builder.Property(x => x.Notes).HasColumnName("notes");

        builder.HasOne(x => x.Medication).WithMany().HasForeignKey(x => x.MedicationId).IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PharmaceuticalForm).WithMany().HasForeignKey(x => x.PharmaceuticalFormId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.DoseUnit).WithMany().HasForeignKey(x => x.DoseUnitId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Frequency).WithMany().HasForeignKey(x => x.FrequencyId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.AdministrationRoute).WithMany().HasForeignKey(x => x.AdministrationRouteId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
