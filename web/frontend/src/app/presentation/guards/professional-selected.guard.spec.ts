import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree, provideRouter } from '@angular/router';
import { LoggedProfessionalService } from '../../application/logged-professional.service';
import { Professional } from '../../domain/models/professional';
import { LoggedProfessionalStore } from '../../domain/repositories/logged-professional-store';
import { professionalSelectedGuard } from './professional-selected.guard';

class FakeLoggedProfessionalStore extends LoggedProfessionalStore {
  private value: Professional | null = null;
  get() { return this.value; }
  set(professional: Professional) { this.value = professional; }
  clear() { this.value = null; }
}

describe('professionalSelectedGuard', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: LoggedProfessionalStore, useClass: FakeLoggedProfessionalStore }]
    });
  });

  const run = () => TestBed.runInInjectionContext(() =>
    professionalSelectedGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot));

  it('redirects to /profissional when no professional is chosen', () => {
    const result = run() as UrlTree;
    expect(TestBed.inject(Router).serializeUrl(result)).toBe('/profissional');
  });

  it('allows navigation when a professional is chosen', () => {
    TestBed.inject(LoggedProfessionalService).select(
      { id: 1, fullName: 'Ana Souza', council: 'CRM', councilNumber: '12345', councilState: 'SP' });
    expect(run()).toBe(true);
  });
});
