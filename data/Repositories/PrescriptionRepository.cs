using Microsoft.EntityFrameworkCore;
using Prescription.Application.Abstractions;
using Prescription.Data.Context;
using Prescription.Domain.Entities;

namespace Prescription.Data.Repositories;

internal sealed class PrescriptionRepository(PrescriptionDbContext context) : IPrescriptionRepository
{
    public IQueryable<MedicalPrescription> Query() =>
        context.Prescriptions.AsNoTracking()
               .OrderByDescending(p => p.IssuedAt)
               .ThenByDescending(p => p.Id);

    public Task<MedicalPrescription?> GetByIdAsync(uint id, CancellationToken cancellationToken = default) =>
        context.Prescriptions
               .Include(p => p.Client)
               .Include(p => p.Professional)
               .Include(p => p.Institution)
               .Include(p => p.Items).ThenInclude(i => i.Medication)
               .Include(p => p.Items).ThenInclude(i => i.PharmaceuticalForm)
               .Include(p => p.Items).ThenInclude(i => i.DoseUnit)
               .Include(p => p.Items).ThenInclude(i => i.Frequency)
               .Include(p => p.Items).ThenInclude(i => i.AdministrationRoute)
               .AsSplitQuery()
               .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public void Add(MedicalPrescription prescription) => context.Prescriptions.Add(prescription);

    public void Remove(MedicalPrescription prescription) => context.Prescriptions.Remove(prescription);
}
