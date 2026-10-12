namespace Prescription.Domain.Exceptions;

/// <summary>
/// Validation helpers that mirror the database constraints and throw <see cref="DomainException"/>.
/// </summary>
internal static class Guard
{
    public static string Required(string? value, int maxLength, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"Campo '{field}' é obrigatório.");

        return MaxLength(value, maxLength, field);
    }

    public static string RequiredText(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"Campo '{field}' é obrigatório.");

        return value;
    }

    public static string? Optional(string? value, int maxLength, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return MaxLength(value, maxLength, field);
    }

    public static string? OptionalText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    public static string Digits(string? value, int length, string field)
    {
        var required = RequiredText(value, field);

        if (required.Length != length || !required.All(char.IsAsciiDigit))
            throw new DomainException($"Campo '{field}' deve ter {length} dígitos.");

        return required;
    }

    public static uint Id(uint value, string field)
    {
        if (value == 0)
            throw new DomainException($"Campo '{field}' é obrigatório.");

        return value;
    }

    public static uint? OptionalPositive(uint? value, string field)
    {
        if (value == 0)
            throw new DomainException($"Campo '{field}' deve ser maior que zero.");

        return value;
    }

    public static TEnum Defined<TEnum>(TEnum value, string field) where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
            throw new DomainException($"Campo '{field}' possui valor inválido.");

        return value;
    }

    private static string MaxLength(string value, int maxLength, string field)
    {
        if (value.Length > maxLength)
            throw new DomainException($"Campo '{field}' deve ter no máximo {maxLength} caracteres.");

        return value;
    }
}
