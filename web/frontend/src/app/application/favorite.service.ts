import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Medication } from '../domain/models/catalog';
import { FavoriteRepository } from '../domain/repositories/favorite-repository';

/** ECF.10.10 – favorite medications of the logged professional. */
@Injectable({ providedIn: 'root' })
export class FavoriteService {
  private readonly repository = inject(FavoriteRepository);

  list(): Observable<Medication[]> {
    return this.repository.list();
  }

  add(medicationId: number): Observable<void> {
    return this.repository.add(medicationId);
  }

  remove(medicationId: number): Observable<void> {
    return this.repository.remove(medicationId);
  }
}
