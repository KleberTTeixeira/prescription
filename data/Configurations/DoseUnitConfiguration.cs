using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prescription.Domain.Entities;

namespace Prescription.Data.Configurations;

internal sealed class DoseUnitConfiguration : IEntityTypeConfiguration<DoseUnit>
{
    public void Configure(EntityTypeBuilder<DoseUnit> builder)
    {
        builder.ToTable("dose_units");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.Abbreviation).HasColumnName("abbreviation").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(60).IsRequired();
    }
}
