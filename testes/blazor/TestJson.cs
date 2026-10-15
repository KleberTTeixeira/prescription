namespace Prescription.Blazor.Tests;

/// <summary>API answers used by the page tests.</summary>
internal static class TestJson
{
    private const string Address =
        """{ "street": "Rua A", "number": "1", "complement": null, "district": "Centro", "city": "São Paulo", "state": "SP", "zipCode": "01000000" }""";

    public const string Clients =
        $$"""[{ "id": 1, "fullName": "Maria Silva", "birthDate": "1980-01-01", "sex": "Female", "address": {{Address}} }]""";

    public const string Institutions =
        $$"""[{ "id": 1, "name": "Clínica Central", "cnes": "1234567", "address": {{Address}}, "phone": "1133334444" }]""";

    public const string PharmaceuticalForms = """[{ "id": 2, "name": "Cápsula" }, { "id": 4, "name": "Solução" }]""";
    public const string AdministrationRoutes = """[{ "id": 1, "name": "Oral" }]""";
    public const string DoseUnits = """[{ "id": 7, "abbreviation": "cp", "name": "Comprimido" }]""";
    public const string Frequencies = """[{ "id": 3, "name": "8/8 horas" }]""";
    public const string Templates =
        """[{ "id": 2, "title": "Gripe", "content": "Repouso e hidratação.", "createdAt": "2026-10-01T09:30:00", "updatedAt": null }]""";

    public const string Amoxicillin =
        """
        { "id": 5, "name": "Amoxicilina", "activeIngredient": "amoxicilina", "concentration": "500 mg",
          "displayName": "Amoxicilina 500 mg", "pharmaceuticalFormId": 2, "pharmaceuticalFormName": "Cápsula",
          "validityPeriodDays": 10, "isControlled": false, "isAntimicrobial": true }
        """;

    public const string Clonazepam =
        """
        { "id": 16, "name": "Clonazepam", "activeIngredient": "clonazepam", "concentration": "2 mg",
          "displayName": "Clonazepam 2 mg", "pharmaceuticalFormId": 2, "pharmaceuticalFormName": "Comprimido",
          "validityPeriodDays": 30, "isControlled": true, "isAntimicrobial": false }
        """;

    /// <summary>Prescription 3 of professional 1 (Ana), not issued, one catalog item.</summary>
    public static string Prescription(bool issued = false, uint professionalId = 1, string format = "Structured",
                                      string? freeText = null) =>
        $$"""
          { "id": 3, "clientId": 1, "clientName": "Maria Silva", "professionalId": {{professionalId}},
            "professionalName": "Ana Souza", "institutionId": 1, "institutionName": "Clínica Central",
            "format": "{{format}}", "category": "Antimicrobial",
            "freeText": {{(freeText is null ? "null" : $"\"{freeText}\"")}},
            "issuedAt": "2026-10-05T16:20:00", "printedAt": {{(issued ? "\"2026-10-05T16:35:00\"" : "null")}},
            "isIssued": {{issued.ToString().ToLowerInvariant()}},
            "items": [{ "id": 7, "medicationId": 5, "medicationName": "Amoxicilina 500 mg", "pharmaceuticalFormId": 2,
                        "pharmaceuticalFormName": "Cápsula", "dose": 1.500, "doseUnitId": 7, "doseUnitAbbreviation": "cp",
                        "frequencyId": 3, "frequencyName": "8/8 horas", "startDate": "2026-10-05", "durationDays": 7,
                        "isContinuousUse": false, "administrationRouteId": 1, "administrationRouteName": "Oral",
                        "usageDeadline": "2026-10-15", "notes": "Após as refeições" }] }
          """;
}
