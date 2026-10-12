using Prescription.Application.Abstractions;

namespace Prescription.Application.Catalog;

/// <summary>ECF.10.03 b – search active medications by name or active ingredient.</summary>
public sealed class SearchMedications(IMedicationRepository medications)
{
    public async Task<IReadOnlyList<MedicationDto>> ExecuteAsync(string? text,
                                                                 CancellationToken cancellationToken = default) =>
        (await medications.SearchActiveAsync(text?.Trim(), cancellationToken)).Select(m => m.ToDto()).ToList();
}
