namespace Prescription.Blazor.Models;

/// <summary>ECF.10.02 – standard text of the logged professional.</summary>
public sealed record Template(uint Id, string Title, string Content, DateTime CreatedAt, DateTime? UpdatedAt);

public sealed record TemplateRequest(string Title, string Content);
