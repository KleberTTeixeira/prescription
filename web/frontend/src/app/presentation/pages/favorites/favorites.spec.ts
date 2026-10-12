import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { Medication } from '../../../domain/models/catalog';
import { FavoriteRepository } from '../../../domain/repositories/favorite-repository';
import { MedicationRepository } from '../../../domain/repositories/medication-repository';
import { Favorites } from './favorites';

const AMOXICILLIN: Medication = {
  id: 5, name: 'Amoxicilina', activeIngredient: 'Amoxicilina', concentration: '500 mg', displayName: 'Amoxicilina 500 mg',
  pharmaceuticalFormId: 2, pharmaceuticalFormName: 'Cápsula', validityPeriodDays: 10, isControlled: false, isAntimicrobial: true
};

class FakeFavoriteRepository {
  added: number[] = [];
  removed: number[] = [];
  list() { return of([AMOXICILLIN]); }
  add(id: number) {
    this.added.push(id);
    return of(undefined);
  }
  remove(id: number) {
    this.removed.push(id);
    return of(undefined);
  }
}

describe('Favorites (ECF.10.10)', () => {
  function render() {
    const repository = new FakeFavoriteRepository();
    TestBed.configureTestingModule({
      imports: [Favorites],
      providers: [
        { provide: FavoriteRepository, useValue: repository },
        { provide: MedicationRepository, useValue: { search: () => of([AMOXICILLIN]), usageDeadline: () => of(null) } }
      ]
    });
    const fixture = TestBed.createComponent(Favorites);
    fixture.detectChanges();
    return { fixture, element: fixture.nativeElement as HTMLElement, repository };
  }

  it('lists the favorites', () => {
    const { element } = render();
    expect(element.querySelectorAll('[data-testid="favorite"]').length).toBe(1);
    expect(element.textContent).toContain('Amoxicilina 500 mg');
  });

  it('adds a medication found in the search', () => {
    const { fixture, element, repository } = render();
    const input = element.querySelector('[data-testid="medication-term"]') as HTMLInputElement;
    input.value = 'amox';
    input.dispatchEvent(new Event('input'));
    element.querySelector('app-medication-search form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
    (element.querySelector('.medication-row') as HTMLElement).click();
    fixture.detectChanges();
    expect(repository.added).toEqual([5]);
    expect(element.querySelector('[data-testid="message"]')?.textContent).toContain('adicionado aos favoritos');
  });

  it('removes a favorite', () => {
    const { fixture, element, repository } = render();
    (element.querySelector('[data-testid="remove"]') as HTMLButtonElement).click();
    fixture.detectChanges();
    expect(repository.removed).toEqual([5]);
  });
});
