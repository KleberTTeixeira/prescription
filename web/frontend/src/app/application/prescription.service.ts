import { Injectable, inject } from '@angular/core';
import { Observable, of } from 'rxjs';
import {
  Prescription,
  PrescriptionCategory,
  PrescriptionListQuery,
  PrescriptionPage,
  PrescriptionPrintData,
  PrescriptionRequest
} from '../domain/models/prescription';
import { PrescriptionRepository } from '../domain/repositories/prescription-repository';

/** yyyy-MM-dd of a local date. */
export function toIsoDate(date: Date): string {
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${date.getFullYear()}-${month}-${day}`;
}

@Injectable({ providedIn: 'root' })
export class PrescriptionService {
  private readonly repository = inject(PrescriptionRepository);

  static readonly PAGE_SIZE = 20;

  list(query: PrescriptionListQuery): Observable<PrescriptionPage> {
    return this.repository.list(query);
  }

  get(id: number): Observable<Prescription> {
    return this.repository.get(id);
  }

  save(id: number | null, request: PrescriptionRequest): Observable<Prescription> {
    return id === null ? this.repository.create(request) : this.repository.update(id, request);
  }

  delete(id: number): Observable<void> {
    return this.repository.delete(id);
  }

  printData(id: number): Observable<PrescriptionPrintData> {
    return this.repository.printData(id);
  }

  confirmPrint(id: number): Observable<Prescription> {
    return this.repository.confirmPrint(id);
  }

  /** No catalog medication (unstructured or only not registered): Common. */
  suggestCategory(medicationIds: number[]): Observable<PrescriptionCategory> {
    return medicationIds.length === 0 ? of('Common') : this.repository.suggestCategory(medicationIds);
  }

  /** Base of the usage deadline: issue date of the prescription (today for a new one). */
  issueDate(prescription: Prescription | null, today: Date = new Date()): string {
    return prescription ? prescription.issuedAt.slice(0, 10) : toIsoDate(today);
  }
}
