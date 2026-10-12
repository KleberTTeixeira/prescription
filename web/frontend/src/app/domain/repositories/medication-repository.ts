import { Observable } from 'rxjs';
import { Medication } from '../models/catalog';

/** Contract for the medication catalog (implemented in infrastructure). */
export abstract class MedicationRepository {
  /** ECF.10.03 – active medications by name or active ingredient. */
  abstract search(term: string): Observable<Medication[]>;

  /** ECF.10.03 – suggested usage deadline (issue date + days of the medication; null = no limit). */
  abstract usageDeadline(medicationId: number, issueDate: string): Observable<string | null>;
}
