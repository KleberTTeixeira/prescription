using Prescription.Domain.Entities;
using Prescription.Domain.ValueObjects;

namespace Prescription.Tests.Domain;

/// <summary>Valid objects used as a base by the domain tests.</summary>
internal static class TestData
{
    public static readonly DateTime Now = new(2026, 10, 11, 10, 0, 0);

    public static Address ValidAddress() =>
        new("Rua das Flores", "100", "Sala 2", "Centro", "São Paulo", "SP", "01001000");

    public static PrescriptionItem ValidItem(string medicationName = "Amoxicilina 500 mg") =>
        new(medicationId: 1, medicationName, pharmaceuticalFormId: 1, dose: 1m, doseUnitId: 1,
            frequencyId: 1, startDate: new DateOnly(2026, 10, 11), durationDays: 7,
            isContinuousUse: false, administrationRouteId: 1,
            usageDeadline: new DateOnly(2026, 10, 21), notes: null);

    public static string Text(int length) => new('a', length);
}
