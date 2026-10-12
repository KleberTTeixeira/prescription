import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { Medication } from '../../domain/models/catalog';
import { MedicationRepository } from '../../domain/repositories/medication-repository';

@Injectable()
export class MedicationHttpRepository extends MedicationRepository {
  private readonly http = inject(HttpClient);

  search(term: string): Observable<Medication[]> {
    return this.http.get<Medication[]>('/api/medications', { params: { search: term } });
  }

  usageDeadline(medicationId: number, issueDate: string): Observable<string | null> {
    return this.http
      .get<{ usageDeadline: string | null }>(`/api/medications/${medicationId}/usage-deadline`, {
        params: { issueDate }
      })
      .pipe(map(response => response.usageDeadline));
  }
}
