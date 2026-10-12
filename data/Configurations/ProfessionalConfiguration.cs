using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prescription.Domain.Entities;

namespace Prescription.Data.Configurations;

internal sealed class ProfessionalConfiguration : IEntityTypeConfiguration<Professional>
{
    public void Configure(EntityTypeBuilder<Professional> builder)
    {
        builder.ToTable("professionals");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(150).IsRequired();
        builder.Property(x => x.Council).HasColumnName("council").HasMaxLength(10).IsRequired();
        builder.Property(x => x.CouncilNumber).HasColumnName("council_number").HasMaxLength(20).IsRequired();
        builder.Property(x => x.CouncilState).HasColumnName("council_state").HasMaxLength(2).IsFixedLength().IsRequired();
    }
}
