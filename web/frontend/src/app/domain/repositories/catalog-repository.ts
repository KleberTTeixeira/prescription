import { Observable } from 'rxjs';
import { Client, DoseUnit, Institution, Lookup } from '../models/catalog';

/** Contract for reading the support lists (implemented in infrastructure). */
export abstract class CatalogRepository {
  abstract clients(): Observable<Client[]>;
  abstract institutions(): Observable<Institution[]>;
  abstract pharmaceuticalForms(): Observable<Lookup[]>;
  abstract administrationRoutes(): Observable<Lookup[]>;
  abstract doseUnits(): Observable<DoseUnit[]>;
  abstract frequencies(): Observable<Lookup[]>;
}
