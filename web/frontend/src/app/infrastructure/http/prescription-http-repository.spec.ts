import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { PrescriptionCategory, PrescriptionPage } from '../../domain/models/prescription';
import { PrescriptionHttpRepository } from './prescription-http-repository';

describe('PrescriptionHttpRepository', () => {
  function setup() {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), PrescriptionHttpRepository]
    });
    return {
      repository: TestBed.inject(PrescriptionHttpRepository),
      controller: TestBed.inject(HttpTestingController)
    };
  }

  it('lists with OData ($count, $top, $skip, $filter) and maps the rows; date without offset', () => {
    const { repository, controller } = setup();
    let page: PrescriptionPage | undefined;

    repository
      .list({ clientName: " D'Ávila ", sort: { field: 'clientName', direction: 'asc' }, skip: 20, top: 20 })
      .subscribe(value => (page = value));

    const request = controller.expectOne(r => r.url === '/odata/Prescriptions');
    expect(request.request.params.get('$count')).toBe('true');
    expect(request.request.params.get('$orderby')).toBe('ClientName asc,Id asc');
    expect(request.request.params.get('$top')).toBe('20');
    expect(request.request.params.get('$skip')).toBe('20');
    expect(request.request.params.get('$filter')).toBe("contains(ClientName,'D''Ávila')");
    request.flush({
      '@odata.count': 21,
      value: [
        {
          Id: 4, ClientName: 'Rafael', InstitutionName: 'Clínica', Format: 'Structured', Category: 'SpecialControl',
          IssuedAt: '2026-10-05T16:20:00-03:00', IsIssued: false, ItemCount: 2
        }
      ]
    });

    expect(page).toEqual({
      total: 21,
      rows: [
        {
          id: 4, clientName: 'Rafael', institutionName: 'Clínica', format: 'Structured', category: 'SpecialControl',
          issuedAt: '2026-10-05T16:20:00', isIssued: false, itemCount: 2
        }
      ]
    });
    controller.verify();
  });

  it('lists without $filter when the client name is empty', () => {
    const { repository, controller } = setup();
    repository.list({ clientName: '  ', sort: { field: 'id', direction: 'desc' }, skip: 0, top: 20 }).subscribe();
    const request = controller.expectOne(r => r.url === '/odata/Prescriptions');
    expect(request.request.params.has('$filter')).toBe(false);
    expect(request.request.params.get('$orderby')).toBe('Id desc');
    request.flush({ value: [] });
    controller.verify();
  });

  it('CRUD and print endpoints', () => {
    const { repository, controller } = setup();
    repository.get(2).subscribe();
    repository.create({} as never).subscribe();
    repository.update(2, {} as never).subscribe();
    repository.delete(2).subscribe();
    repository.printData(2).subscribe();
    repository.confirmPrint(2).subscribe();

    expect(controller.expectOne({ method: 'GET', url: '/api/prescriptions/2' })).toBeTruthy();
    expect(controller.expectOne({ method: 'POST', url: '/api/prescriptions' })).toBeTruthy();
    expect(controller.expectOne({ method: 'PUT', url: '/api/prescriptions/2' })).toBeTruthy();
    expect(controller.expectOne({ method: 'DELETE', url: '/api/prescriptions/2' })).toBeTruthy();
    expect(controller.expectOne({ method: 'GET', url: '/api/prescriptions/2/print-data' })).toBeTruthy();
    expect(controller.expectOne({ method: 'POST', url: '/api/prescriptions/2/print-confirmation' })).toBeTruthy();
  });

  it('suggested category sends one medicationIds per id', () => {
    const { repository, controller } = setup();
    let category: PrescriptionCategory | undefined;
    repository.suggestCategory([1, 5]).subscribe(value => (category = value));

    const request = controller.expectOne(r => r.url === '/api/prescriptions/suggested-category');
    expect(request.request.params.getAll('medicationIds')).toEqual(['1', '5']);
    request.flush({ category: 'Antimicrobial' });

    expect(category).toBe('Antimicrobial');
    controller.verify();
  });
});
