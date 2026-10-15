using Prescription.Blazor.Models;

namespace Prescription.Blazor.Tests;

internal static class TestData
{
    public static readonly Professional Ana = new(1, "Ana Souza", "CRM", "12345", "SP");
    public static readonly Professional Bruno = new(2, "Bruno Lima", "CRM", "67890", "RJ");

    public const string ProfessionalsJson =
        """
        [
          { "id": 1, "fullName": "Ana Souza", "council": "CRM", "councilNumber": "12345", "councilState": "SP" },
          { "id": 2, "fullName": "Bruno Lima", "council": "CRM", "councilNumber": "67890", "councilState": "RJ" }
        ]
        """;
}
