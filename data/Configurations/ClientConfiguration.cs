using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prescription.Data.Converters;
using Prescription.Domain.Entities;

namespace Prescription.Data.Configurations;

internal sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(150).IsRequired();
        builder.Property(x => x.BirthDate).HasColumnName("birth_date").HasColumnType("date")
               .HasConversion(DateOnlyConverters.Required).IsRequired();
        builder.Property(x => x.Sex).HasColumnName("sex").HasConversion(EnumConverters.Sex)
               .HasMaxLength(1).IsFixedLength().IsRequired();
        builder.OwnsOne(x => x.Address, AddressMapping.Map);
        builder.Navigation(x => x.Address).IsRequired();
    }
}
