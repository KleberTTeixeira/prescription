import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { Medication } from '../../../domain/models/catalog';
import { MedicationRepository } from '../../../domain/repositories/medication-repository';
import { MedicationSearch } from './medication-search';

const AMOXICILLIN: Medication = {
  id: 5, name: 'Amoxicilina', activeIngredient: 'Amoxicilina', concentration: '500 mg', displayName: 'Amoxicilina 500 mg',
  pharmaceuticalFormId: 2, pharmaceuticalFormName: 'Cápsula', validityPeriodDays: 10, isControlled: false, isAntimicrobial: true
};
const DIPYRONE: Medication = {
  id: 1, name: 'Dipirona Sódica', activeIngredient: 'Dipirona', concentration: '500 mg', displayName: 'Dipirona Sódica 500 mg',
  pharmaceuticalFormId: 1, pharmaceuticalFormName: 'Comprimido', validityPeriodDays: null, isControlled: false, isAntimicrobial: false
};

class FakeMedicationRepository extends MedicationRepository {
  terms: string[] = [];
  search(term: string) {
    this.terms.push(term);
    return of([AMOXICILLIN, DIPYRONE]);
  }
  usageDeadline() {
    return of(null);
  }
}

describe('MedicationSearch (ECF.10.07)', () => {
  function render() {
    const repository = new FakeMedicationRepository();
    TestBed.configureTestingModule({
      imports: [MedicationSearch],
      providers: [{ provide: MedicationRepository, useValue: repository }]
    });
    const fixture = TestBed.createComponent(MedicationSearch);
    fixture.detectChanges();
    const element = fixture.nativeElement as HTMLElement;
    const search = (term: string) => {
      const input = element.querySelector('[data-testid="medication-term"]') as HTMLInputElement;
      input.value = term;
      input.dispatchEvent(new Event('input'));
      element.querySelector('form')!.dispatchEvent(new Event('submit'));
      fixture.detectChanges();
    };
    return { fixture, element, repository, search };
  }

  it('searches by the typed term and shows one distinct row per medication', () => {
    const { element, repository, search } = render();
    search(' amox ');
    expect(repository.terms).toEqual(['amox']);
    const rows = element.querySelectorAll('.medication-row');
    expect(rows.length).toBe(2);
    expect(rows[0].textContent).toContain('Amoxicilina 500 mg');
    expect(rows[0].textContent).toContain('Antimicrobiano');
  });

  it('empty term does not call the API', () => {
    const { element, repository, search } = render();
    search('   ');
    expect(repository.terms).toEqual([]);
    expect(element.querySelector('.medication-list')).toBeNull();
  });

  it('click marks the chosen row and emits the medication', () => {
    const { fixture, element, search } = render();
    const chosen: Medication[] = [];
    fixture.componentInstance.selected.subscribe(medication => chosen.push(medication));
    search('a');

    (element.querySelectorAll('.medication-row')[1] as HTMLElement).click();
    fixture.detectChanges();

    expect(chosen).toEqual([DIPYRONE]);
    const rows = element.querySelectorAll('.medication-row');
    expect(rows[1].classList.contains('chosen')).toBe(true);
    expect(rows[0].classList.contains('chosen')).toBe(false);
  });
});
