import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import {
  Prescription,
  PrescriptionCategory,
  PrescriptionFormat,
  PrescriptionListQuery,
  PrescriptionPage,
  PrescriptionPrintData,
  PrescriptionRequest,
  PrescriptionSort,
  PrescriptionSortField
} from '../../domain/models/prescription';
import { PrescriptionRepository } from '../../domain/repositories/prescription-repository';

/** Row as returned by GET /odata/Prescriptions (OData keeps the C# property names). */
interface ODataPrescriptionRow {
  Id: number;
  ClientName: string;
  InstitutionName: string;
  Format: PrescriptionFormat;
  Category: PrescriptionCategory;
  IssuedAt: string;
  IsIssued: boolean;
  ItemCount: number;
}

interface ODataResponse<T> {
  '@odata.count'?: number;
  value: T[];
}

/**
 * OData writes the date with the Brasília offset ("2026-10-05T16:20:00-03:00"); /api writes it without offset.
 * Both are kept as the same local text ("2026-10-05T16:20:00").
 */
function withoutOffset(dateTime: string): string {
  return dateTime.slice(0, 19);
}

const ODATA_FIELDS: Record<PrescriptionSortField, string> = {
  id: 'Id',
  issuedAt: 'IssuedAt',
  clientName: 'ClientName',
  format: 'Format',
  category: 'Category',
  itemCount: 'ItemCount',
  isIssued: 'IsIssued'
};

/** Ties are ordered by Id (same direction) so paging does not repeat or skip rows. */
function toOrderBy(sort: PrescriptionSort): string {
  const orderBy = `${ODATA_FIELDS[sort.field]} ${sort.direction}`;
  return sort.field === 'id' ? orderBy : `${orderBy},Id ${sort.direction}`;
}

@Injectable()
export class PrescriptionHttpRepository extends PrescriptionRepository {
  private readonly http = inject(HttpClient);

  list(query: PrescriptionListQuery): Observable<PrescriptionPage> {
    let params = new HttpParams()
      .set('$count', 'true')
      .set('$orderby', toOrderBy(query.sort))
      .set('$top', query.top)
      .set('$skip', query.skip);
    const clientName = query.clientName.trim();
    if (clientName !== '') {
      params = params.set('$filter', `contains(ClientName,'${clientName.replaceAll("'", "''")}')`);
    }

    return this.http.get<ODataResponse<ODataPrescriptionRow>>('/odata/Prescriptions', { params }).pipe(
      map(response => ({
        total: response['@odata.count'] ?? response.value.length,
        rows: response.value.map(row => ({
          id: row.Id,
          clientName: row.ClientName,
          institutionName: row.InstitutionName,
          format: row.Format,
          category: row.Category,
          issuedAt: withoutOffset(row.IssuedAt),
          isIssued: row.IsIssued,
          itemCount: row.ItemCount
        }))
      }))
    );
  }

  get(id: number): Observable<Prescription> {
    return this.http.get<Prescription>(`/api/prescriptions/${id}`);
  }

  create(request: PrescriptionRequest): Observable<Prescription> {
    return this.http.post<Prescription>('/api/prescriptions', request);
  }

  update(id: number, request: PrescriptionRequest): Observable<Prescription> {
    return this.http.put<Prescription>(`/api/prescriptions/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`/api/prescriptions/${id}`);
  }

  printData(id: number): Observable<PrescriptionPrintData> {
    return this.http.get<PrescriptionPrintData>(`/api/prescriptions/${id}/print-data`);
  }

  confirmPrint(id: number): Observable<Prescription> {
    return this.http.post<Prescription>(`/api/prescriptions/${id}/print-confirmation`, null);
  }

  suggestCategory(medicationIds: number[]): Observable<PrescriptionCategory> {
    const params = new HttpParams({ fromObject: { medicationIds: medicationIds.map(String) } });
    return this.http
      .get<{ category: PrescriptionCategory }>('/api/prescriptions/suggested-category', { params })
      .pipe(map(response => response.category));
  }
}
