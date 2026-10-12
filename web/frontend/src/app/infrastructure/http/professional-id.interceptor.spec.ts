import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { LoggedProfessionalService } from '../../application/logged-professional.service';
import { Professional } from '../../domain/models/professional';
import { LoggedProfessionalStore } from '../../domain/repositories/logged-professional-store';
import { PROFESSIONAL_ID_HEADER, professionalIdInterceptor } from './professional-id.interceptor';

const PROFESSIONAL: Professional = {
  id: 2, fullName: 'Ana Souza', council: 'CRM', councilNumber: '12345', councilState: 'SP'
};

class FakeLoggedProfessionalStore extends LoggedProfessionalStore {
  private value: Professional | null = null;
  get() { return this.value; }
  set(professional: Professional) { this.value = professional; }
  clear() { this.value = null; }
}

describe('professionalIdInterceptor', () => {
  let http: HttpClient;
  let controller: HttpTestingController;
  let loggedProfessional: LoggedProfessionalService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([professionalIdInterceptor])),
        provideHttpClientTesting(),
        { provide: LoggedProfessionalStore, useClass: FakeLoggedProfessionalStore }
      ]
    });
    http = TestBed.inject(HttpClient);
    controller = TestBed.inject(HttpTestingController);
    loggedProfessional = TestBed.inject(LoggedProfessionalService);
  });

  afterEach(() => controller.verify());

  it('adds the header to /api requests when a professional is chosen', () => {
    loggedProfessional.select(PROFESSIONAL);
    http.get('/api/prescriptions').subscribe();
    expect(controller.expectOne('/api/prescriptions').request.headers.get(PROFESSIONAL_ID_HEADER)).toBe('2');
  });

  it('adds the header to /odata requests when a professional is chosen', () => {
    loggedProfessional.select(PROFESSIONAL);
    http.get('/odata/Prescriptions').subscribe();
    expect(controller.expectOne('/odata/Prescriptions').request.headers.get(PROFESSIONAL_ID_HEADER)).toBe('2');
  });

  it('does not add the header when no professional is chosen', () => {
    http.get('/api/professionals').subscribe();
    expect(controller.expectOne('/api/professionals').request.headers.has(PROFESSIONAL_ID_HEADER)).toBe(false);
  });

  it('does not add the header to requests outside the API', () => {
    loggedProfessional.select(PROFESSIONAL);
    http.get('/health').subscribe();
    expect(controller.expectOne('/health').request.headers.has(PROFESSIONAL_ID_HEADER)).toBe(false);
  });
});
