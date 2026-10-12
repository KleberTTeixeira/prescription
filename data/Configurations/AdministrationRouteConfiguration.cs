using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prescription.Domain.Entities;

namespace Prescription.Data.Configurations;

internal sealed class AdministrationRouteConfiguration : IEntityTypeConfiguration<AdministrationRoute>
{
    public void Configure(EntityTypeBuilder<AdministrationRoute> builder)
    {
        builder.ToTable("administration_routes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(60).IsRequired();
    }
}
