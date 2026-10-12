using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prescription.Domain.Entities;

namespace Prescription.Data.Configurations;

internal sealed class FavoriteMedicationConfiguration : IEntityTypeConfiguration<FavoriteMedication>
{
    public void Configure(EntityTypeBuilder<FavoriteMedication> builder)
    {
        builder.ToTable("favorite_medications");
        builder.HasKey(x => new { x.ProfessionalId, x.MedicationId });
        builder.Property(x => x.ProfessionalId).HasColumnName("professional_id");
        builder.Property(x => x.MedicationId).HasColumnName("medication_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasOne(x => x.Professional).WithMany().HasForeignKey(x => x.ProfessionalId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Medication).WithMany().HasForeignKey(x => x.MedicationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
