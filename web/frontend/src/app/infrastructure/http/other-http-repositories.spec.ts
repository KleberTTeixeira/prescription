import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { CatalogHttpRepository } from './catalog-http-repository';
import { FavoriteHttpRepository } from './favorite-http-repository';
import { MedicationHttpRepository } from './medication-http-repository';
import { TemplateHttpRepository } from './template-http-repository';

describe('Catalog, medication, template and favorite HTTP repositories', () => {
  function setup() {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        CatalogHttpRepository,
        MedicationHttpRepository,
        TemplateHttpRepository,
        FavoriteHttpRepository
      ]
    });
    return TestBed.inject(HttpTestingController);
  }

  it('catalog lists', () => {
    const controller = setup();
    const repository = TestBed.inject(CatalogHttpRepository);
    repository.clients().subscribe();
    repository.institutions().subscribe();
    repository.pharmaceuticalForms().subscribe();
    repository.administrationRoutes().subscribe();
    repository.doseUnits().subscribe();
    repository.frequencies().subscribe();

    for (const url of [
      '/api/clients', '/api/institutions', '/api/pharmaceutical-forms',
      '/api/administration-routes', '/api/dose-units', '/api/frequencies'
    ]) {
      controller.expectOne({ method: 'GET', url });
    }
    controller.verify();
  });

  it('medication search and suggested usage deadline', () => {
    const controller = setup();
    const repository = TestBed.inject(MedicationHttpRepository);
    let deadline: string | null | undefined;

    repository.search('amox').subscribe();
    repository.usageDeadline(5, '2026-10-05').subscribe(value => (deadline = value));

    expect(controller.expectOne(r => r.url === '/api/medications').request.params.get('search')).toBe('amox');
    const request = controller.expectOne(r => r.url === '/api/medications/5/usage-deadline');
    expect(request.request.params.get('issueDate')).toBe('2026-10-05');
    request.flush({ usageDeadline: '2026-10-15' });

    expect(deadline).toBe('2026-10-15');
    controller.verify();
  });

  it('templates CRUD', () => {
    const controller = setup();
    const repository = TestBed.inject(TemplateHttpRepository);
    repository.list().subscribe();
    repository.create({ title: 'A', content: 'B' }).subscribe();
    repository.update(3, { title: 'A', content: 'B' }).subscribe();
    repository.delete(3).subscribe();

    controller.expectOne({ method: 'GET', url: '/api/templates' });
    expect(controller.expectOne({ method: 'POST', url: '/api/templates' }).request.body).toEqual({ title: 'A', content: 'B' });
    controller.expectOne({ method: 'PUT', url: '/api/templates/3' });
    controller.expectOne({ method: 'DELETE', url: '/api/templates/3' });
    controller.verify();
  });

  it('favorites list, add (PUT) and remove (DELETE)', () => {
    const controller = setup();
    const repository = TestBed.inject(FavoriteHttpRepository);
    repository.list().subscribe();
    repository.add(5).subscribe();
    repository.remove(5).subscribe();

    controller.expectOne({ method: 'GET', url: '/api/favorites' });
    controller.expectOne({ method: 'PUT', url: '/api/favorites/5' });
    controller.expectOne({ method: 'DELETE', url: '/api/favorites/5' });
    controller.verify();
  });
});
