using Prescription.Application.Abstractions;
using Prescription.Application.Catalog;
using Prescription.Domain.Enums;

namespace Prescription.Application.Prescriptions;

/// <summary>
/// ECF.10.04 – print data (any professional can reprint). Special control adds the client address;
/// antimicrobial adds the client age (on the print date) and sex.
/// </summary>
public sealed class GetPrescriptionPrintData(IPrescriptionRepository prescriptions, IClock clock)
{
    public async Task<PrescriptionPrintDto> ExecuteAsync(uint id, CancellationToken cancellationToken = default)
    {
        var p = await prescriptions.GetByIdAsync(id, cancellationToken) ?? throw PrescriptionOwnership.NotFound(id);

        var isSpecialControl = p.Category == PrescriptionCategory.SpecialControl;
        var isAntimicrobial = p.Category == PrescriptionCategory.Antimicrobial;

        var client = new PrintClientDto(
            p.Client.FullName,
            isSpecialControl ? p.Client.Address.ToDto() : null,
            isAntimicrobial ? AgeOn(p.Client.BirthDate, clock.Today) : null,
            isAntimicrobial ? p.Client.Sex : null);

        return new PrescriptionPrintDto(p.Id, p.Format, p.Category, p.FreeText, p.IssuedAt, p.PrintedAt, client,
                                        p.Professional.ToDto(), p.Institution.ToDto(), p.Items.ToDtos());
    }

    private static int AgeOn(DateOnly birthDate, DateOnly date)
    {
        var age = date.Year - birthDate.Year;
        return birthDate > date.AddYears(-age) ? age - 1 : age;
    }
}
