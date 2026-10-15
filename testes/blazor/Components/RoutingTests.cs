using Microsoft.AspNetCore.Components;
using Prescription.Blazor.Components.Pages;

namespace Prescription.Blazor.Tests.Components;

public class RoutingTests : BlazorTestContext
{
    [Fact]
    public void Root_OpensPrescriptions()
    {
        Render<Home>();

        Assert.Equal("/receitas", CurrentPath);
    }

    [Fact]
    public void UnknownRoute_OpensPrescriptions()
    {
        Render<NotFound>();

        Assert.Equal("/receitas", CurrentPath);
    }

    [Theory]
    [InlineData(typeof(ProfessionalSelection), "/profissional")]
    [InlineData(typeof(Prescriptions), "/receitas")]
    [InlineData(typeof(Templates), "/textos-padrao")]
    [InlineData(typeof(Favorites), "/favoritos")]
    [InlineData(typeof(PrescriptionForm), "/receitas/nova", "/receitas/{Id:int}/editar")]
    [InlineData(typeof(PrescriptionView), "/receitas/{Id:int}")]
    [InlineData(typeof(PrescriptionPrint), "/receitas/{Id:int}/imprimir")]
    public void Routes_InPortuguese(Type page, params string[] routes)
    {
        var templates = Attribute.GetCustomAttributes(page, typeof(RouteAttribute))
                                 .Cast<RouteAttribute>()
                                 .Select(attribute => attribute.Template);

        Assert.Equal(routes.Order(), templates.Order());
    }
}
