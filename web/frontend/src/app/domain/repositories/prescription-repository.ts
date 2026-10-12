import { Observable } from 'rxjs';
import {
  Prescription,
  PrescriptionCategory,
  PrescriptionListQuery,
  PrescriptionPage,
  PrescriptionPrintData,
  PrescriptionRequest
} from '../models/prescription';

/** Contract for the prescriptions of the logged professional (implemented in infrastructure). */
export abstract class PrescriptionRepository {
  abstract list(query: PrescriptionListQuery): Observable<PrescriptionPage>;
  abstract get(id: number): Observable<Prescription>;
  abstract create(request: PrescriptionRequest): Observable<Prescription>;
  abstract update(id: number, request: PrescriptionRequest): Observable<Prescription>;
  abstract delete(id: number): Observable<void>;
  abstract printData(id: number): Observable<PrescriptionPrintData>;
  /** Issues the prescription after the user confirms the print. */
  abstract confirmPrint(id: number): Observable<Prescription>;
  abstract suggestCategory(medicationIds: number[]): Observable<PrescriptionCategory>;
}
