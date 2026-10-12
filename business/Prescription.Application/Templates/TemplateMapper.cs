using Prescription.Domain.Entities;

namespace Prescription.Application.Templates;

internal static class TemplateMapper
{
    public static TemplateDto ToDto(this PrescriptionTemplate t) => new(t.Id, t.Title, t.Content, t.CreatedAt, t.UpdatedAt);
}
