import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PrescriptionTemplate, TemplateRequest } from '../domain/models/template';
import { TemplateRepository } from '../domain/repositories/template-repository';

@Injectable({ providedIn: 'root' })
export class TemplateService {
  private readonly repository = inject(TemplateRepository);

  list(): Observable<PrescriptionTemplate[]> {
    return this.repository.list();
  }

  save(id: number | null, request: TemplateRequest): Observable<PrescriptionTemplate> {
    return id === null ? this.repository.create(request) : this.repository.update(id, request);
  }

  delete(id: number): Observable<void> {
    return this.repository.delete(id);
  }
}
