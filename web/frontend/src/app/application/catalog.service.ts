import { Injectable, inject } from '@angular/core';
import { Observable, forkJoin } from 'rxjs';
import { Client, DoseUnit, Institution, Lookup } from '../domain/models/catalog';
import { CatalogRepository } from '../domain/repositories/catalog-repository';

/** All support lists used by the prescription form. */
export interface PrescriptionFormLists {
  clients: Client[];
  institutions: Institution[];
  pharmaceuticalForms: Lookup[];
  administrationRoutes: Lookup[];
  doseUnits: DoseUnit[];
  frequencies: Lookup[];
}

@Injectable({ providedIn: 'root' })
export class CatalogService {
  private readonly repository = inject(CatalogRepository);

  formLists(): Observable<PrescriptionFormLists> {
    return forkJoin({
      clients: this.repository.clients(),
      institutions: this.repository.institutions(),
      pharmaceuticalForms: this.repository.pharmaceuticalForms(),
      administrationRoutes: this.repository.administrationRoutes(),
      doseUnits: this.repository.doseUnits(),
      frequencies: this.repository.frequencies()
    });
  }
}
