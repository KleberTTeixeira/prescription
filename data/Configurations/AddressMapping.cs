using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prescription.Domain.ValueObjects;

namespace Prescription.Data.Configurations;

/// <summary>Address value object stored in the address_* columns of the owner table (clients, institutions).</summary>
internal static class AddressMapping
{
    public static void Map<TOwner>(OwnedNavigationBuilder<TOwner, Address> address) where TOwner : class
    {
        address.Property(a => a.Street).HasColumnName("address_street").HasMaxLength(150).IsRequired();
        address.Property(a => a.Number).HasColumnName("address_number").HasMaxLength(10).IsRequired();
        address.Property(a => a.Complement).HasColumnName("address_complement").HasMaxLength(60);
        address.Property(a => a.District).HasColumnName("address_district").HasMaxLength(80).IsRequired();
        address.Property(a => a.City).HasColumnName("address_city").HasMaxLength(80).IsRequired();
        address.Property(a => a.State).HasColumnName("address_state").HasMaxLength(2).IsFixedLength().IsRequired();
        address.Property(a => a.ZipCode).HasColumnName("address_zip_code").HasMaxLength(8).IsFixedLength().IsRequired();
    }
}
