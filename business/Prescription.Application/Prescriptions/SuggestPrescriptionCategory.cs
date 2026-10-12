using Prescription.Application.Abstractions;
using Prescription.Domain.Enums;

namespace Prescription.Application.Prescriptions;

/// <summary>
/// Category suggested by the system (the user can change it): any controlled medication → special control;
/// otherwise any antimicrobial → antimicrobial; otherwise common. No catalog medication → common.
/// </summary>
public sealed class SuggestPrescriptionCategory(IMedicationRepository medications)
{
    public async Task<PrescriptionCategory> ExecuteAsync(IEnumerable<uint>? medicationIds,
                                                         CancellationToken cancellationToken = default)
    {
        var ids = medicationIds?.Distinct().ToList() ?? [];
        if (ids.Count == 0)
            return PrescriptionCategory.Common;

        var found = await medications.GetByIdsAsync(ids, cancellationToken);

        if (found.Any(m => m.IsControlled))
            return PrescriptionCategory.SpecialControl;

        if (found.Any(m => m.IsAntimicrobial))
            return PrescriptionCategory.Antimicrobial;

        return PrescriptionCategory.Common;
    }
}
