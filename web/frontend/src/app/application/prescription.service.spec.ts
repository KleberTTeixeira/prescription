import { TestBed } from '@angular/core/testing';
import { Observable, of } from 'rxjs';
import { Prescription, PrescriptionCategory } from '../domain/models/prescription';
import { PrescriptionRepository } from '../domain/repositories/prescription-repository';
import { PrescriptionService, toIsoDate } from './prescription.service';

class FakePrescriptionRepository {
  suggestCalls: number[][] = [];
  created = 0;
  updated = 0;
  suggestCategory(ids: number[]): Observable<PrescriptionCategory> {
    this.suggestCalls.push(ids);
    return of('SpecialControl');
  }
  create() {
    this.created++;
    return of({ id: 10 } as Prescription);
  }
  update() {
    this.updated++;
    return of({ id: 3 } as Prescription);
  }
}

describe('PrescriptionService', () => {
  function setup() {
    const repository = new FakePrescriptionRepository();
    TestBed.configureTestingModule({ providers: [{ provide: PrescriptionRepository, useValue: repository }] });
    return { service: TestBed.inject(PrescriptionService), repository };
  }

  it('suggested category without catalog medication is Common (no API call)', () => {
    const { service, repository } = setup();
    let category: PrescriptionCategory | undefined;
    service.suggestCategory([]).subscribe(value => (category = value));
    expect(category).toBe('Common');
    expect(repository.suggestCalls).toEqual([]);
  });

  it('suggested category with catalog medications comes from the API', () => {
    const { service, repository } = setup();
    let category: PrescriptionCategory | undefined;
    service.suggestCategory([1, 5]).subscribe(value => (category = value));
    expect(category).toBe('SpecialControl');
    expect(repository.suggestCalls).toEqual([[1, 5]]);
  });

  it('save: no id creates, with id updates', () => {
    const { service, repository } = setup();
    service.save(null, {} as never).subscribe();
    service.save(3, {} as never).subscribe();
    expect(repository.created).toBe(1);
    expect(repository.updated).toBe(1);
  });

  it('issue date: today for a new prescription, issuedAt date when editing', () => {
    const { service } = setup();
    expect(service.issueDate(null, new Date(2026, 9, 5, 23, 59))).toBe('2026-10-05');
    expect(service.issueDate({ issuedAt: '2026-10-01T16:20:00' } as Prescription)).toBe('2026-10-01');
  });

  it('toIsoDate pads month and day', () => {
    expect(toIsoDate(new Date(2026, 0, 2))).toBe('2026-01-02');
  });
});
