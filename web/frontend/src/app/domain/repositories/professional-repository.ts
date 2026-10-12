import { Observable } from 'rxjs';
import { Professional } from '../models/professional';

/** Contract for reading professionals (implemented in infrastructure). */
export abstract class ProfessionalRepository {
  abstract list(): Observable<Professional[]>;
}
