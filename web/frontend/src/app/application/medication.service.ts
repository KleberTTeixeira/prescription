import { Injectable, inject } from '@angular/core';
import { Observable, of } from 'rxjs';
import { Medication } from '../domain/models/catalog';
import { MedicationRepository } from '../domain/repositories/medication-repository';

@Injectable({ providedIn: 'root' })
export class MedicationService {
  private readonly repository = inject(MedicationRepository);

  /** Empty term: no search (empty list). */
  search(term: string): Observable<Medication[]> {
    const trimmed = term.trim();
    return trimmed === '' ? of([]) : this.repository.search(trimmed);
  }

  /** ECF.10.03 – issue date + days of the medication (null = no limit; the field stays blank). */
  usageDeadline(medicationId: number, issueDate: string): Observable<string | null> {
    return this.repository.usageDeadline(medicationId, issueDate);
  }
}
