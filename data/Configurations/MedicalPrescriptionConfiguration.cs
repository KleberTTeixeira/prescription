using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prescription.Data.Converters;
using Prescription.Domain.Entities;

namespace Prescription.Data.Configurations;

internal sealed class MedicalPrescriptionConfiguration : IEntityTypeConfiguration<MedicalPrescription>
{
    public void Configure(EntityTypeBuilder<MedicalPrescription> builder)
    {
        builder.ToTable("prescriptions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.ClientId).HasColumnName("client_id");
        builder.Property(x => x.ProfessionalId).HasColumnName("professional_id");
        builder.Property(x => x.InstitutionId).HasColumnName("institution_id");
        builder.Property(x => x.Format).HasColumnName("format").HasConversion(EnumConverters.Format)
               .HasMaxLength(12).IsRequired();
        builder.Property(x => x.Category).HasColumnName("category").HasConversion(EnumConverters.Category)
               .HasMaxLength(16).IsRequired();
        builder.Property(x => x.FreeText).HasColumnName("free_text");
        builder.Property(x => x.IssuedAt).HasColumnName("issued_at");
        builder.Property(x => x.PrintedAt).HasColumnName("printed_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Ignore(x => x.IsIssued);

        builder.HasOne(x => x.Client).WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Professional).WithMany().HasForeignKey(x => x.ProfessionalId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Institution).WithMany().HasForeignKey(x => x.InstitutionId)
               .OnDelete(DeleteBehavior.Restrict);

        // Items are changed only through the aggregate (private list _items).
        builder.HasMany(x => x.Items).WithOne().HasForeignKey(i => i.PrescriptionId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Items).HasField("_items").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
