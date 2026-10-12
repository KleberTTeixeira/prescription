import { Observable } from 'rxjs';
import { Medication } from '../models/catalog';

/** Contract for the favorite medications of the logged professional – ECF.10.10 (implemented in infrastructure). */
export abstract class FavoriteRepository {
  abstract list(): Observable<Medication[]>;
  abstract add(medicationId: number): Observable<void>;
  abstract remove(medicationId: number): Observable<void>;
}
