import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Client, DoseUnit, Institution, Lookup } from '../../domain/models/catalog';
import { CatalogRepository } from '../../domain/repositories/catalog-repository';

@Injectable()
export class CatalogHttpRepository extends CatalogRepository {
  private readonly http = inject(HttpClient);

  clients(): Observable<Client[]> {
    return this.http.get<Client[]>('/api/clients');
  }

  institutions(): Observable<Institution[]> {
    return this.http.get<Institution[]>('/api/institutions');
  }

  pharmaceuticalForms(): Observable<Lookup[]> {
    return this.http.get<Lookup[]>('/api/pharmaceutical-forms');
  }

  administrationRoutes(): Observable<Lookup[]> {
    return this.http.get<Lookup[]>('/api/administration-routes');
  }

  doseUnits(): Observable<DoseUnit[]> {
    return this.http.get<DoseUnit[]>('/api/dose-units');
  }

  frequencies(): Observable<Lookup[]> {
    return this.http.get<Lookup[]>('/api/frequencies');
  }
}
