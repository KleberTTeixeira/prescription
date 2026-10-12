using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Prescription.Data.Converters;

/// <summary>
/// MySql.EntityFrameworkCore 9.0.17 reads DATE columns as DateTime and does not convert to DateOnly
/// (InvalidCastException found in the integration tests). These converters keep DateOnly in the domain.
/// </summary>
internal static class DateOnlyConverters
{
    public static readonly ValueConverter<DateOnly, DateTime> Required = new(
        d => d.ToDateTime(TimeOnly.MinValue),
        d => DateOnly.FromDateTime(d));

    public static readonly ValueConverter<DateOnly?, DateTime?> Optional = new(
        d => d.HasValue ? d.Value.ToDateTime(TimeOnly.MinValue) : null,
        d => d.HasValue ? DateOnly.FromDateTime(d.Value) : null);
}
