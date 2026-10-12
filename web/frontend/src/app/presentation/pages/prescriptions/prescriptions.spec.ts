import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { PrescriptionListItem, PrescriptionListQuery } from '../../../domain/models/prescription';
import { PrescriptionRepository } from '../../../domain/repositories/prescription-repository';
import { ConfirmService } from '../../components/confirm-dialog/confirm-dialog';
import { Prescriptions } from './prescriptions';

const ROWS: PrescriptionListItem[] = [
  { id: 4, clientName: 'Rafael', institutionName: 'Clínica', format: 'Structured', category: 'SpecialControl',
    issuedAt: '2026-10-05T16:20:00', isIssued: false, itemCount: 2 },
  { id: 2, clientName: 'Maria', institutionName: 'Clínica', format: 'Unstructured', category: 'Common',
    issuedAt: '2026-10-01T09:00:00', isIssued: true, itemCount: 0 }
];

class FakePrescriptionRepository {
  queries: PrescriptionListQuery[] = [];
  deleted: number[] = [];
  list(query: PrescriptionListQuery) {
    this.queries.push(query);
    return of({ rows: ROWS, total: 2 });
  }
  delete(id: number) {
    this.deleted.push(id);
    return of(undefined);
  }
}

describe('Prescriptions (list)', () => {
  function render(confirmed: boolean) {
    const repository = new FakePrescriptionRepository();
    const confirm = { ask: vi.fn().mockResolvedValue(confirmed) };
    TestBed.configureTestingModule({
      imports: [Prescriptions],
      providers: [
        provideRouter([]),
        { provide: PrescriptionRepository, useValue: repository },
        { provide: ConfirmService, useValue: confirm }
      ]
    });
    const fixture = TestBed.createComponent(Prescriptions);
    fixture.detectChanges();
    return { fixture, element: fixture.nativeElement as HTMLElement, repository, confirm };
  }

  it('loads the first page, most recent first, and shows the rows with status', () => {
    const { element, repository } = render(true);
    expect(repository.queries).toEqual([
      { clientName: '', sort: { field: 'issuedAt', direction: 'desc' }, skip: 0, top: 20 }
    ]);
    expect(element.querySelector('[data-testid="sort-issuedAt"]')?.textContent).toContain('▼');
    const rows = element.querySelectorAll('[data-testid="row"]');
    expect(rows.length).toBe(2);
    expect(rows[0].textContent).toContain('05/10/2026 16:20');
    expect(rows[0].textContent).toContain('Controle especial');
    expect(rows[0].textContent).toContain('Não emitida');
    expect(rows[1].textContent).toContain('Emitida');
  });

  it('edit only for a not issued prescription', () => {
    const { element } = render(true);
    const rows = element.querySelectorAll('[data-testid="row"]');
    expect(rows[0].querySelector('[data-testid="edit"]')).not.toBeNull();
    expect(rows[1].querySelector('[data-testid="edit"]')).toBeNull();
  });

  it('filters by client name', () => {
    const { fixture, element, repository } = render(true);
    const input = element.querySelector('input[type="search"]') as HTMLInputElement;
    input.value = 'Raf';
    input.dispatchEvent(new Event('input'));
    element.querySelector('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
    expect(repository.queries[1]).toEqual({
      clientName: 'Raf', sort: { field: 'issuedAt', direction: 'desc' }, skip: 0, top: 20
    });
  });

  it('click on a column title sorts by it; second click inverts; indicator follows', () => {
    const { fixture, element, repository } = render(true);
    const header = () => element.querySelector('[data-testid="sort-clientName"]') as HTMLButtonElement;

    header().click();
    fixture.detectChanges();
    expect(repository.queries[1].sort).toEqual({ field: 'clientName', direction: 'asc' });
    expect(header().textContent).toContain('▲');
    expect(element.querySelector('[data-testid="sort-issuedAt"]')?.textContent).not.toContain('▼');
    expect(header().closest('th')?.getAttribute('aria-sort')).toBe('ascending');

    header().click();
    fixture.detectChanges();
    expect(repository.queries[2].sort).toEqual({ field: 'clientName', direction: 'desc' });
    expect(header().textContent).toContain('▼');
  });

  it('every column title can be clicked to sort', () => {
    const { element } = render(true);
    for (const field of ['id', 'issuedAt', 'clientName', 'format', 'category', 'itemCount', 'isIssued']) {
      expect(element.querySelector(`[data-testid="sort-${field}"]`)).not.toBeNull();
    }
  });

  it('deletes only after confirmation', async () => {
    const confirmedCase = render(true);
    (confirmedCase.element.querySelector('[data-testid="delete"]') as HTMLButtonElement).click();
    await confirmedCase.fixture.whenStable();
    expect(confirmedCase.confirm.ask).toHaveBeenCalled();
    expect(confirmedCase.repository.deleted).toEqual([4]);
  });

  it('does not delete when the user cancels', async () => {
    const { fixture, element, repository } = render(false);
    (element.querySelector('[data-testid="delete"]') as HTMLButtonElement).click();
    await fixture.whenStable();
    expect(repository.deleted).toEqual([]);
  });
});
