namespace Prescription.Application.Templates;

public sealed record TemplateRequest(string Title, string Content);

public sealed record TemplateDto(uint Id, string Title, string Content, DateTime CreatedAt, DateTime? UpdatedAt);
