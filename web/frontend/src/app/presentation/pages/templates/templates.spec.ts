import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { PrescriptionTemplate, TemplateRequest } from '../../../domain/models/template';
import { TemplateRepository } from '../../../domain/repositories/template-repository';
import { ConfirmService } from '../../components/confirm-dialog/confirm-dialog';
import { Templates } from './templates';

const TEMPLATE: PrescriptionTemplate = { id: 1, title: 'Repouso', content: 'Repouso por 3 dias.', createdAt: '', updatedAt: null };

class FakeTemplateRepository {
  created: TemplateRequest[] = [];
  updated: [number, TemplateRequest][] = [];
  deleted: number[] = [];
  list() { return of([TEMPLATE]); }
  create(request: TemplateRequest) {
    this.created.push(request);
    return of({ ...TEMPLATE, id: 2, ...request });
  }
  update(id: number, request: TemplateRequest) {
    this.updated.push([id, request]);
    return of({ ...TEMPLATE, ...request });
  }
  delete(id: number) {
    this.deleted.push(id);
    return of(undefined);
  }
}

describe('Templates (ECF.10.02)', () => {
  function render() {
    const repository = new FakeTemplateRepository();
    TestBed.configureTestingModule({
      imports: [Templates],
      providers: [
        { provide: TemplateRepository, useValue: repository },
        { provide: ConfirmService, useValue: { ask: vi.fn().mockResolvedValue(true) } }
      ]
    });
    const fixture = TestBed.createComponent(Templates);
    fixture.detectChanges();
    return { fixture, element: fixture.nativeElement as HTMLElement, repository };
  }

  function fill(element: HTMLElement, title: string, content: string) {
    const titleInput = element.querySelector('#template-title') as HTMLInputElement;
    const contentInput = element.querySelector('#template-content') as HTMLTextAreaElement;
    titleInput.value = title;
    titleInput.dispatchEvent(new Event('input'));
    contentInput.value = content;
    contentInput.dispatchEvent(new Event('input'));
  }

  it('lists the templates of the professional', () => {
    const { element } = render();
    expect(element.querySelectorAll('[data-testid="template"]').length).toBe(1);
    expect(element.textContent).toContain('Repouso por 3 dias.');
  });

  it('creates a template', () => {
    const { fixture, element, repository } = render();
    (element.querySelector('[data-testid="new"]') as HTMLButtonElement).click();
    fixture.detectChanges();
    fill(element, ' Dieta ', 'Dieta leve.');
    element.querySelector('[data-testid="form"]')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
    expect(repository.created).toEqual([{ title: 'Dieta', content: 'Dieta leve.' }]);
    expect(element.querySelector('[data-testid="message"]')?.textContent).toContain('salvo');
  });

  it('requires title', () => {
    const { fixture, element, repository } = render();
    (element.querySelector('[data-testid="new"]') as HTMLButtonElement).click();
    fixture.detectChanges();
    fill(element, ' ', 'Texto');
    element.querySelector('[data-testid="form"]')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
    expect(element.querySelector('[data-testid="error"]')?.textContent).toContain("Campo 'Título' é obrigatório.");
    expect(repository.created).toEqual([]);
  });

  it('deletes after confirmation', async () => {
    const { fixture, element, repository } = render();
    (element.querySelector('[data-testid="delete"]') as HTMLButtonElement).click();
    await fixture.whenStable();
    expect(repository.deleted).toEqual([1]);
  });
});
