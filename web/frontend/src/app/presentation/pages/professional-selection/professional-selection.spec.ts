import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';
import { LoggedProfessionalService } from '../../../application/logged-professional.service';
import { Professional } from '../../../domain/models/professional';
import { LoggedProfessionalStore } from '../../../domain/repositories/logged-professional-store';
import { ProfessionalRepository } from '../../../domain/repositories/professional-repository';
import { ProfessionalSelection } from './professional-selection';

const PROFESSIONALS: Professional[] = [
  { id: 1, fullName: 'Ana Souza', council: 'CRM', councilNumber: '12345', councilState: 'SP' },
  { id: 2, fullName: 'Bruno Lima', council: 'CRM', councilNumber: '54321', councilState: 'RJ' }
];

class FakeLoggedProfessionalStore extends LoggedProfessionalStore {
  private value: Professional | null = null;
  get() { return this.value; }
  set(professional: Professional) { this.value = professional; }
  clear() { this.value = null; }
}

class FakeProfessionalRepository extends ProfessionalRepository {
  constructor(private readonly result: Observable<Professional[]>) { super(); }
  list() { return this.result; }
}

describe('ProfessionalSelection', () => {
  function render(result: Observable<Professional[]>) {
    TestBed.configureTestingModule({
      imports: [ProfessionalSelection],
      providers: [
        provideRouter([]),
        { provide: LoggedProfessionalStore, useClass: FakeLoggedProfessionalStore },
        { provide: ProfessionalRepository, useValue: new FakeProfessionalRepository(result) }
      ]
    });
    const fixture = TestBed.createComponent(ProfessionalSelection);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  }

  it('lists the professionals with council data', () => {
    const element = render(of(PROFESSIONALS));
    const items = element.querySelectorAll('.list-group-item');
    expect(items.length).toBe(2);
    expect(items[0].textContent).toContain('Ana Souza');
    expect(items[0].textContent).toContain('CRM 12345/SP');
  });

  it('chooses the clicked professional and goes to /receitas', () => {
    const element = render(of(PROFESSIONALS));
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    (element.querySelectorAll('.list-group-item')[1] as HTMLButtonElement).click();

    expect(TestBed.inject(LoggedProfessionalService).professional()).toEqual(PROFESSIONALS[1]);
    expect(navigate).toHaveBeenCalledWith(['/receitas']);
  });

  it('shows the API error message', () => {
    const error = new HttpErrorResponse({ status: 500, error: { detail: 'Falha ao listar.' } });
    const element = render(throwError(() => error));
    expect(element.querySelector('[data-testid="error"]')?.textContent).toContain('Falha ao listar.');
    expect(element.querySelector('.list-group')).toBeNull();
  });
});
