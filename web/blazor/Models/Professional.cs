namespace Prescription.Blazor.Models;

/// <summary>Professional returned by GET /api/professionals.</summary>
public sealed record Professional(uint Id, string FullName, string Council, string CouncilNumber, string CouncilState)
{
    /// <summary>Council, number and state, e.g. "CRM 12345/SP".</summary>
    public string CouncilRegistration => $"{Council} {CouncilNumber}/{CouncilState}";

    /// <summary>Avatar text: first letters of the first two names, e.g. "Ana Beatriz Moreira" → "AB".</summary>
    public string Initials =>
        string.Concat(FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2)
                              .Select(part => char.ToUpperInvariant(part[0])));
}
