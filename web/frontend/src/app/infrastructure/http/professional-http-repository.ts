import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Professional } from '../../domain/models/professional';
import { ProfessionalRepository } from '../../domain/repositories/professional-repository';

@Injectable()
export class ProfessionalHttpRepository extends ProfessionalRepository {
  private readonly http = inject(HttpClient);

  list(): Observable<Professional[]> {
    return this.http.get<Professional[]>('/api/professionals');
  }
}
