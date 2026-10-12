import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PrescriptionTemplate, TemplateRequest } from '../../domain/models/template';
import { TemplateRepository } from '../../domain/repositories/template-repository';

@Injectable()
export class TemplateHttpRepository extends TemplateRepository {
  private readonly http = inject(HttpClient);

  list(): Observable<PrescriptionTemplate[]> {
    return this.http.get<PrescriptionTemplate[]>('/api/templates');
  }

  create(request: TemplateRequest): Observable<PrescriptionTemplate> {
    return this.http.post<PrescriptionTemplate>('/api/templates', request);
  }

  update(id: number, request: TemplateRequest): Observable<PrescriptionTemplate> {
    return this.http.put<PrescriptionTemplate>(`/api/templates/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`/api/templates/${id}`);
  }
}
