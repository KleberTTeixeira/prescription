using Prescription.Domain.Exceptions;

namespace Prescription.Domain.Entities;

/// <summary>Table prescription_templates – standard texts of each professional (ECF.10.02).</summary>
public sealed class PrescriptionTemplate
{
    public uint Id { get; private set; }
    public uint ProfessionalId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public Professional Professional { get; private set; } = null!;

    private PrescriptionTemplate() { }

    public PrescriptionTemplate(uint professionalId, string title, string content, DateTime now)
    {
        ProfessionalId = Guard.Id(professionalId, "Profissional");
        SetText(title, content);
        CreatedAt = now;
    }

    public void Update(string title, string content, DateTime now)
    {
        SetText(title, content);
        UpdatedAt = now;
    }

    private void SetText(string title, string content)
    {
        Title = Guard.Required(title, 100, "Título do texto padrão");
        Content = Guard.RequiredText(content, "Conteúdo do texto padrão");
    }
}
