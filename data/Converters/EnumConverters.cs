using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Prescription.Domain.Enums;

namespace Prescription.Data.Converters;

/// <summary>Converts the domain enums to the text values allowed by the CHECK constraints of the database.</summary>
internal static class EnumConverters
{
    public static readonly ValueConverter<PrescriptionFormat, string> Format = new(
        v => v == PrescriptionFormat.Structured ? "STRUCTURED" : "UNSTRUCTURED",
        v => v == "STRUCTURED" ? PrescriptionFormat.Structured : PrescriptionFormat.Unstructured);

    public static readonly ValueConverter<PrescriptionCategory, string> Category = new(
        v => v == PrescriptionCategory.SpecialControl ? "SPECIAL_CONTROL"
           : v == PrescriptionCategory.Antimicrobial ? "ANTIMICROBIAL"
           : "COMMON",
        v => v == "SPECIAL_CONTROL" ? PrescriptionCategory.SpecialControl
           : v == "ANTIMICROBIAL" ? PrescriptionCategory.Antimicrobial
           : PrescriptionCategory.Common);

    public static readonly ValueConverter<Sex, string> Sex = new(
        v => v == Domain.Enums.Sex.Female ? "F" : "M",
        v => v == "F" ? Domain.Enums.Sex.Female : Domain.Enums.Sex.Male);
}
