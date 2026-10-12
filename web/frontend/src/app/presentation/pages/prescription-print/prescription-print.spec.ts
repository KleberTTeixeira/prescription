import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { Prescription, PrescriptionPrintData } from '../../../domain/models/prescription';
import { Professional } from '../../../domain/models/professional';
import { LoggedProfessionalStore } from '../../../domain/repositories/logged-professional-store';
import { PrescriptionRepository } from '../../../domain/repositories/prescription-repository';
import { ConfirmService } from '../../components/confirm-dialog/confirm-dialog';
import { PRINT_WINDOW, PrescriptionPrint } from './prescription-print';

const PROFESSIONAL: Professional = { id: 1, fullName: 'Ana Souza', council: 'CRM', councilNumber: '12345', councilState: 'SP' };
const ADDRESS = { street: 'Rua A', number: '10', complement: null, district: 'Centro', city: 'São Paulo', state: 'SP', zipCode: '01000000' };

function printData(overrides: Partial<PrescriptionPrintData> = {}): PrescriptionPrintData {
  return {
    id: 2, format: 'Structured', category: 'Antimicrobial', freeText: null,
    issuedAt: '2026-10-05T16:20:00', printedAt: null,
    client: { fullName: 'Maria', address: null, age: 46, sex: 'Female' },
    professional: PROFESSIONAL,
    institution: { id: 1, name: 'Clínica Central', cnes: '1234567', address: ADDRESS, phone: '1133334444' },
    items: [
      {
        id: 1, medicationId: 5, medicationName: 'Amoxicilina 500 mg', pharmaceuticalFormId: 2, pharmaceuticalFormName: 'Cápsula',
        dose: 1, doseUnitId: 7, doseUnitAbbreviation: 'cp', frequencyId: 3, frequencyName: '8/8 horas', startDate: '2026-10-05',
        durationDays: 7, isContinuousUse: false, administrationRouteId: 1, administrationRouteName: 'Oral',
        usageDeadline: '2026-10-15', notes: null
      }
    ],
    ...overrides
  };
}

class FakeStore extends LoggedProfessionalStore {
  get() { return PROFESSIONAL; }
  set() {}
  clear() {}
}

describe('PrescriptionPrint (ECF.10.04)', () => {
  function render(data: PrescriptionPrintData, confirmed = true) {
    const confirmPrint = vi.fn(() => of({ id: data.id } as Prescription));
    const repository = { printData: () => of(data), confirmPrint };
    const confirm = { ask: vi.fn().mockResolvedValue(confirmed) };
    const print = vi.fn();
    TestBed.configureTestingModule({
      imports: [PrescriptionPrint],
      providers: [
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap({ id: String(data.id) }) } } },
        { provide: PrescriptionRepository, useValue: repository },
        { provide: ConfirmService, useValue: confirm },
        { provide: PRINT_WINDOW, useValue: print },
        { provide: LoggedProfessionalStore, useClass: FakeStore }
      ]
    });
    const fixture = TestBed.createComponent(PrescriptionPrint);
    fixture.detectChanges();
    return { fixture, element: fixture.nativeElement as HTMLElement, confirmPrint, confirm, print };
  }

  async function clickPrint(fixture: ReturnType<typeof render>['fixture']) {
    (fixture.nativeElement.querySelector('[data-testid="print"]') as HTMLButtonElement).click();
    await fixture.whenStable();
    fixture.detectChanges();
  }

  it('shows institution (CNES, address, phone), professional (council/number/state), date and items', () => {
    const { element } = render(printData());
    const sheet = element.querySelector('[data-testid="sheet"]')!.textContent!;
    expect(sheet).toContain('Clínica Central');
    expect(sheet).toContain('CNES 1234567');
    expect(sheet).toContain('Rua A, 10, Centro, São Paulo/SP, CEP 01000-000');
    expect(sheet).toContain('Telefone: 1133334444');
    expect(sheet).toContain('CRM 12345/SP');
    expect(sheet).toContain('05/10/2026 16:20');
    expect(sheet).toContain('Amoxicilina 500 mg');
    expect(sheet).toContain('Usar até 15/10/2026');
  });

  it('antimicrobial: client age and sex; no address', () => {
    const { element } = render(printData());
    expect(element.querySelector('[data-testid="client-age"]')?.textContent).toContain('46 anos');
    expect(element.querySelector('[data-testid="client-sex"]')?.textContent).toContain('Feminino');
    expect(element.querySelector('[data-testid="client-address"]')).toBeNull();
  });

  it('special control: client address', () => {
    const { element } = render(
      printData({ category: 'SpecialControl', client: { fullName: 'Maria', address: ADDRESS, age: null, sex: null } })
    );
    expect(element.querySelector('[data-testid="client-address"]')?.textContent).toContain('Rua A, 10');
    expect(element.querySelector('[data-testid="client-age"]')).toBeNull();
  });

  it('prints, asks "A receita foi impressa?" and issues when confirmed', async () => {
    const { fixture, element, confirm, confirmPrint, print } = render(printData());
    await clickPrint(fixture);
    expect(print).toHaveBeenCalled();
    expect(confirm.ask).toHaveBeenCalledWith(expect.objectContaining({ message: 'A receita foi impressa?' }));
    expect(confirmPrint).toHaveBeenCalledWith(2);
    expect(element.querySelector('[data-testid="message"]')?.textContent).toContain('Receita emitida.');
  });

  it('does not issue when the user answers "Não"', async () => {
    const { fixture, confirmPrint } = render(printData(), false);
    await clickPrint(fixture);
    expect(confirmPrint).not.toHaveBeenCalled();
  });

  it('already issued: reprints without asking', async () => {
    const { fixture, confirm, confirmPrint, print } = render(printData({ printedAt: '2026-10-05T16:35:00' }));
    await clickPrint(fixture);
    expect(print).toHaveBeenCalled();
    expect(confirm.ask).not.toHaveBeenCalled();
    expect(confirmPrint).not.toHaveBeenCalled();
  });

  it('prescription of another professional: reprints without asking', async () => {
    const other = { ...PROFESSIONAL, id: 2 };
    const { fixture, confirm } = render(printData({ professional: other }));
    await clickPrint(fixture);
    expect(confirm.ask).not.toHaveBeenCalled();
  });
});
