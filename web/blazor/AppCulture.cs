using System.Globalization;

namespace Prescription.Blazor;

/// <summary>Screens in Portuguese: dates dd/MM/yyyy, decimal comma and month names of the date pickers.</summary>
public static class AppCulture
{
    public static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>Number without the zeros the API keeps (dose DECIMAL(10,3)): 1.500 → "1,5"; 2.000 → "2".</summary>
    public static string Number(decimal value) => value.ToString("0.###", PtBr);
}
