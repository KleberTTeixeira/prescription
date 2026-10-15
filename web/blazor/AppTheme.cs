using MudBlazor;

namespace Prescription.Blazor;

/// <summary>Colors of the screens (health blue).</summary>
public static class AppTheme
{
    public static readonly MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#1565c0",
            Secondary = "#616161",
            AppbarBackground = "#1565c0",
            Background = "#f5f6fa",
            DrawerBackground = "#ffffff",
            Success = "#2e7d32",
            Warning = "#e65100",
            Error = "#d32f2f"
        },
        LayoutProperties = new LayoutProperties { DefaultBorderRadius = "8px" }
    };
}
