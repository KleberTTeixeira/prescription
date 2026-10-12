import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Professional } from '../domain/models/professional';
import { ProfessionalRepository } from '../domain/repositories/professional-repository';

@Injectable({ providedIn: 'root' })
export class ProfessionalService {
  private readonly repository = inject(ProfessionalRepository);

  list(): Observable<Professional[]> {
    return this.repository.list();
  }
}
