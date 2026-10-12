import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { Medication } from '../../../domain/models/catalog';
import { Prescription, PrescriptionCategory, PrescriptionRequest } from '../../../domain/models/prescription';
import { Professional } from '../../../domain/models/professional';
import { CatalogRepository } from '../../../domain/repositories/catalog-repository';
import { FavoriteRepository } from '../../../domain/repositories/favorite-repository';
import { LoggedProfessionalStore } from '../../../domain/repositories/logged-professional-store';
import { MedicationRepository } from '../../../domain/repositories/medication-repository';
import { PrescriptionRepository } from '../../../domain/repositories/prescription-repository';
import { TemplateRepository } from '../../../domain/repositories/template-repository';
import { PrescriptionForm } from './prescription-form';

const PROFESSIONAL: Professional = { id: 1, fullName: 'Ana', council: 'CRM', councilNumber: '1', councilState: 'SP' };
const AMOXICILLIN: Medication = {
  id: 5, name: 'Amoxicilina', activeIngredient: 'Amoxicilina', concentration: '500 mg', displayName: 'Amoxicilina 500 mg',
  pharmaceuticalFormId: 2, pharmaceuticalFormName: 'Cápsula', validityPeriodDays: 10, isControlled: false, isAntimicrobial: true
};
const ADDRESS = { street: 'Rua A', number: '1', complement: null, district: 'Centro', city: 'São Paulo', state: 'SP', zipCode: '01000000' };

const catalog = {
  clients: () => of([{ id: 1, fullName: 'Maria', birthDate: '1980-01-01', sex: 'Female', address: ADDRESS }]),
  institutions: () => of([{ id: 1, name: 'Clínica', cnes: '1234567', address: ADDRESS, phone: '1133334444' }]),
  pharmaceuticalForms: () => of([{ id: 2, name: 'Cápsula' }, { id: 4, name: 'Solução' }]),
  administrationRoutes: () => of([{ id: 1, name: 'Oral' }]),
  doseUnits: () => of([{ id: 7, abbreviation: 'cp', name: 'Comprimido' }]),
  frequencies: () => of([{ id: 3, name: '8/8 horas' }])
};

class FakePrescriptionRepository {
  created: PrescriptionRequest[] = [];
  updated: PrescriptionRequest[] = [];
  suggestCalls: number[][] = [];
  constructor(private readonly existing: Prescription | null) {}
  get() {
    return of(this.existing as Prescription);
  }
  create(request: PrescriptionRequest) {
    this.created.push(request);
    return of({ id: 10 } as Prescription);
  }
  update(_id: number, request: PrescriptionRequest) {
    this.updated.push(request);
    return of({ id: 3 } as Prescription);
  }
  suggestCategory(ids: number[]) {
    this.suggestCalls.push(ids);
    return of<PrescriptionCategory>('Antimicrobial');
  }
}

class FakeStore extends LoggedProfessionalStore {
  get() { return PROFESSIONAL; }
  set() {}
  clear() {}
}

function existing(overrides: Partial<Prescription> = {}): Prescription {
  return {
    id: 3, clientId: 1, clientName: 'Maria', professionalId: 1, professionalName: 'Ana', institutionId: 1,
    institutionName: 'Clínica', format: 'Unstructured', category: 'Common', freeText: 'Repouso.',
    issuedAt: '2026-10-01T09:00:00', printedAt: null, isIssued: false, items: [], ...overrides
  };
}

describe('PrescriptionForm', () => {
  function render(prescription: Prescription | null = null) {
    const repository = new FakePrescriptionRepository(prescription);
    const deadlineCalls: [number, string][] = [];
    TestBed.configureTestingModule({
      imports: [PrescriptionForm],
      providers: [
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap(prescription ? { id: '3' } : {}) } } },
        { provide: PrescriptionRepository, useValue: repository },
        { provide: CatalogRepository, useValue: catalog },
        { provide: TemplateRepository, useValue: { list: () => of([{ id: 1, title: 'Repouso', content: 'Repouso por 3 dias.', createdAt: '', updatedAt: null }]) } },
        { provide: FavoriteRepository, useValue: { list: () => of([AMOXICILLIN]) } },
        {
          provide: MedicationRepository,
          useValue: {
            search: () => of([]),
            usageDeadline: (id: number, issueDate: string) => {
              deadlineCalls.push([id, issueDate]);
              return of('2026-10-11');
            }
          }
        },
        { provide: LoggedProfessionalStore, useClass: FakeStore }
      ]
    });
    const fixture = TestBed.createComponent(PrescriptionForm);
    fixture.detectChanges();
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    const element = fixture.nativeElement as HTMLElement;
    const component = fixture.componentInstance;
    return { fixture, element, component, repository, deadlineCalls, navigate };
  }

  it('new prescription starts structured with one empty item', () => {
    const { element } = render();
    expect(element.querySelectorAll('[data-testid="item"]').length).toBe(1);
    expect((element.querySelector('#format-structured') as HTMLInputElement).checked).toBe(true);
  });

  it('choosing a favorite fills name, suggested deadline (issue date + days) and suggested category', () => {
    const { fixture, element, component, repository, deadlineCalls } = render();
    const select = element.querySelector('select[aria-label="Escolher dos favoritos"]') as HTMLSelectElement;
    select.value = '5';
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();

    const today = component['prescriptionService'].issueDate(null);
    expect(deadlineCalls).toEqual([[5, today]]);
    expect(element.querySelector('[data-testid="medication-name"]')?.textContent).toContain('Amoxicilina 500 mg');
    expect((element.querySelector('[data-testid="usage-deadline"]') as HTMLInputElement).value).toBe('2026-10-11');
    expect(repository.suggestCalls).toEqual([[5]]);
    expect(component['form'].controls.category.value).toBe('Antimicrobial');
    expect(element.querySelector('[data-testid="suggested-category"]')?.textContent).toContain('Antimicrobiano');
  });

  it('not registered medication: typed name and form, deadline blank', () => {
    const { fixture, element, component } = render();
    (element.querySelector('[data-testid="not-registered"]') as HTMLButtonElement).click();
    fixture.detectChanges();
    expect(element.querySelector('#item-0-name')).not.toBeNull();
    expect(element.querySelector('#item-0-form')).not.toBeNull();
    expect(component['items'].at(0).controls.usageDeadline.value).toBe('');
  });

  it('shows the validation errors and does not call the API', () => {
    const { fixture, element, repository } = render();
    (element.querySelector('#format-unstructured') as HTMLInputElement).click();
    fixture.detectChanges();
    element.querySelector('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    const errors = element.querySelector('[data-testid="errors"]')?.textContent;
    expect(errors).toContain("Campo 'Cliente' é obrigatório.");
    expect(errors).toContain('Receita não estruturada exige texto livre.');
    expect(repository.created).toEqual([]);
  });

  it('ECF.10.02 – uses the standard text as base of the free text', () => {
    const { fixture, element, component } = render();
    component['templateId'].setValue(1);
    (element.querySelector('[data-testid="use-template"]') as HTMLButtonElement).click();
    fixture.detectChanges();
    expect((element.querySelector('#free-text') as HTMLTextAreaElement).value).toBe('Repouso por 3 dias.');
  });

  it('creates an unstructured prescription and opens it', () => {
    const { fixture, element, component, repository, navigate } = render();
    component['form'].patchValue({ clientId: 1, institutionId: 1, format: 'Unstructured', freeText: ' Repouso. ' });
    element.querySelector('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    expect(repository.created).toEqual([
      { clientId: 1, institutionId: 1, format: 'Unstructured', category: 'Common', freeText: 'Repouso.', items: [] }
    ]);
    expect(navigate).toHaveBeenCalledWith(['/receitas', 10]);
  });

  it('edits an existing prescription (loads the data and updates)', () => {
    const { fixture, element, component, repository } = render(existing());
    expect(component['form'].controls.freeText.value).toBe('Repouso.');
    element.querySelector('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
    expect(repository.updated.length).toBe(1);
  });

  it('issued prescription is not opened for editing', () => {
    const { element } = render(existing({ isIssued: true, printedAt: '2026-10-01T09:15:00' }));
    expect(element.querySelector('[data-testid="blocked"]')?.textContent).toContain(
      'Receita já emitida (impressa) não pode ser alterada.'
    );
    expect(element.querySelector('form')).toBeNull();
  });

  it('prescription of another professional is not opened for editing', () => {
    const { element } = render(existing({ professionalId: 2 }));
    expect(element.querySelector('[data-testid="blocked"]')?.textContent).toContain('Receita pertence a outro profissional.');
  });
});
