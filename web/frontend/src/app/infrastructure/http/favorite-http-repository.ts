import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Medication } from '../../domain/models/catalog';
import { FavoriteRepository } from '../../domain/repositories/favorite-repository';

@Injectable()
export class FavoriteHttpRepository extends FavoriteRepository {
  private readonly http = inject(HttpClient);

  list(): Observable<Medication[]> {
    return this.http.get<Medication[]>('/api/favorites');
  }

  add(medicationId: number): Observable<void> {
    return this.http.put<void>(`/api/favorites/${medicationId}`, null);
  }

  remove(medicationId: number): Observable<void> {
    return this.http.delete<void>(`/api/favorites/${medicationId}`);
  }
}
