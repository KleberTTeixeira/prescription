import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Professional } from '../../domain/models/professional';
import { ProfessionalHttpRepository } from './professional-http-repository';

describe('ProfessionalHttpRepository', () => {
  it('lists professionals from GET /api/professionals', () => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), ProfessionalHttpRepository]
    });
    const repository = TestBed.inject(ProfessionalHttpRepository);
    const controller = TestBed.inject(HttpTestingController);
    const professionals: Professional[] = [
      { id: 1, fullName: 'Ana Souza', council: 'CRM', councilNumber: '12345', councilState: 'SP' }
    ];

    let result: Professional[] | undefined;
    repository.list().subscribe(value => (result = value));

    const request = controller.expectOne('/api/professionals');
    expect(request.request.method).toBe('GET');
    request.flush(professionals);

    expect(result).toEqual(professionals);
    controller.verify();
  });
});
