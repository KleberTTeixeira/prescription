using Prescription.Application.Abstractions;
using Prescription.Application.Exceptions;

namespace Prescription.Application.Prescriptions;

/// <summary>
/// ECF.10.03 usage deadline suggested for an item: issue date + validity_periods.days of the medication.
/// Null when the medication has no validity limit. The user can change or clear the date.
/// </summary>
public sealed class SuggestUsageDeadline(IMedicationRepository medications)
{
    public async Task<DateOnly?> ExecuteAsync(uint medicationId, DateOnly issueDate,
                                              CancellationToken cancellationToken = default)
    {
        var medication = await medications.GetByIdAsync(medicationId, cancellationToken)
                         ?? throw new NotFoundException($"Medicamento {medicationId} não encontrado.");

        return medication.ValidityPeriod.Days is { } days ? issueDate.AddDays((int)days) : null;
    }
}
