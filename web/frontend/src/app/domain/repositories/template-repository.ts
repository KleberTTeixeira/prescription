import { Observable } from 'rxjs';
import { PrescriptionTemplate, TemplateRequest } from '../models/template';

/** Contract for the standard texts of the logged professional (implemented in infrastructure). */
export abstract class TemplateRepository {
  abstract list(): Observable<PrescriptionTemplate[]>;
  abstract create(request: TemplateRequest): Observable<PrescriptionTemplate>;
  abstract update(id: number, request: TemplateRequest): Observable<PrescriptionTemplate>;
  abstract delete(id: number): Observable<void>;
}
