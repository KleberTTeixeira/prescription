using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prescription.Domain.Entities;

namespace Prescription.Data.Configurations;

internal sealed class MedicationConfiguration : IEntityTypeConfiguration<Medication>
{
    public void Configure(EntityTypeBuilder<Medication> builder)
    {
        builder.ToTable("medications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(x => x.ActiveIngredient).HasColumnName("active_ingredient").HasMaxLength(150).IsRequired();
        builder.Property(x => x.Concentration).HasColumnName("concentration").HasMaxLength(60);
        builder.Property(x => x.PharmaceuticalFormId).HasColumnName("pharmaceutical_form_id");
        builder.Property(x => x.ValidityPeriodId).HasColumnName("validity_period_id");
        builder.Property(x => x.IsControlled).HasColumnName("is_controlled");
        builder.Property(x => x.IsAntimicrobial).HasColumnName("is_antimicrobial");
        builder.Property(x => x.IsActive).HasColumnName("is_active");

        builder.HasOne(x => x.PharmaceuticalForm).WithMany().HasForeignKey(x => x.PharmaceuticalFormId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ValidityPeriod).WithMany().HasForeignKey(x => x.ValidityPeriodId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
