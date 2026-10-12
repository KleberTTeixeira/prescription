using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prescription.Domain.Entities;

namespace Prescription.Data.Configurations;

internal sealed class ValidityPeriodConfiguration : IEntityTypeConfiguration<ValidityPeriod>
{
    public void Configure(EntityTypeBuilder<ValidityPeriod> builder)
    {
        builder.ToTable("validity_periods");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
        builder.Property(x => x.Days).HasColumnName("days");
    }
}
