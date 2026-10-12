namespace Prescription.Domain.Enums;

/// <summary>Database values: UNSTRUCTURED, STRUCTURED.</summary>
public enum PrescriptionFormat
{
    /// <summary>ECF.10.01 – free text.</summary>
    Unstructured = 1,

    /// <summary>ECF.10.03 – items.</summary>
    Structured = 2
}
