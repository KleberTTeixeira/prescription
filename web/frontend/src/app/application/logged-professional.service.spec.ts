import { TestBed } from '@angular/core/testing';
import { Professional } from '../domain/models/professional';
import { LoggedProfessionalStore } from '../domain/repositories/logged-professional-store';
import { LoggedProfessionalService } from './logged-professional.service';

const PROFESSIONAL: Professional = {
  id: 2, fullName: 'Ana Souza', council: 'CRM', councilNumber: '12345', councilState: 'SP'
};

class FakeLoggedProfessionalStore extends LoggedProfessionalStore {
  value: Professional | null = null;
  get() { return this.value; }
  set(professional: Professional) { this.value = professional; }
  clear() { this.value = null; }
}

describe('LoggedProfessionalService', () => {
  let store: FakeLoggedProfessionalStore;

  function create(): LoggedProfessionalService {
    TestBed.configureTestingModule({ providers: [{ provide: LoggedProfessionalStore, useValue: store }] });
    return TestBed.inject(LoggedProfessionalService);
  }

  beforeEach(() => (store = new FakeLoggedProfessionalStore()));

  it('starts with the professional kept in the store', () => {
    store.value = PROFESSIONAL;
    expect(create().professional()).toEqual(PROFESSIONAL);
  });

  it('starts empty when the store is empty', () => {
    expect(create().professional()).toBeNull();
  });

  it('select keeps the professional in the store and in the state', () => {
    const service = create();
    service.select(PROFESSIONAL);
    expect(service.professional()).toEqual(PROFESSIONAL);
    expect(store.value).toEqual(PROFESSIONAL);
  });

  it('clear removes the professional from the store and the state', () => {
    store.value = PROFESSIONAL;
    const service = create();
    service.clear();
    expect(service.professional()).toBeNull();
    expect(store.value).toBeNull();
  });
});
