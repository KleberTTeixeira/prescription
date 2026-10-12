using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prescription.Domain.Entities;

namespace Prescription.Data.Configurations;

internal sealed class InstitutionConfiguration : IEntityTypeConfiguration<Institution>
{
    public void Configure(EntityTypeBuilder<Institution> builder)
    {
        builder.ToTable("institutions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(x => x.Cnes).HasColumnName("cnes").HasMaxLength(7).IsFixedLength().IsRequired();
        builder.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20).IsRequired();
        builder.OwnsOne(x => x.Address, AddressMapping.Map);
        builder.Navigation(x => x.Address).IsRequired();
    }
}
