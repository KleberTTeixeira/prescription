import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TemplateService } from '../../../application/template.service';
import { PrescriptionTemplate } from '../../../domain/models/template';
import { toErrorMessage } from '../../../infrastructure/http/problem-details';
import { ConfirmService } from '../../components/confirm-dialog/confirm-dialog';

/** ECF.10.02 – standard texts of the logged professional (used as a base in the prescription). */
@Component({
  selector: 'app-templates',
  imports: [ReactiveFormsModule],
  templateUrl: './templates.html'
})
export class Templates implements OnInit {
  private readonly templateService = inject(TemplateService);
  private readonly confirm = inject(ConfirmService);

  protected readonly templates = signal<PrescriptionTemplate[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly message = signal<string | null>(null);
  /** null = form closed; 0 = new; otherwise id being edited. */
  protected readonly editingId = signal<number | null>(null);
  protected readonly saving = signal(false);

  protected readonly form = new FormGroup({
    title: new FormControl('', { nonNullable: true }),
    content: new FormControl('', { nonNullable: true })
  });

  ngOnInit(): void {
    this.load();
  }

  protected create(): void {
    this.form.setValue({ title: '', content: '' });
    this.message.set(null);
    this.error.set(null);
    this.editingId.set(0);
  }

  protected edit(template: PrescriptionTemplate): void {
    this.form.setValue({ title: template.title, content: template.content });
    this.message.set(null);
    this.error.set(null);
    this.editingId.set(template.id);
  }

  protected cancel(): void {
    this.editingId.set(null);
  }

  protected save(): void {
    const { title, content } = this.form.getRawValue();
    if (title.trim() === '' || content.trim() === '') {
      this.error.set(title.trim() === '' ? "Campo 'Título' é obrigatório." : "Campo 'Texto' é obrigatório.");
      return;
    }

    const id = this.editingId();
    this.saving.set(true);
    this.error.set(null);
    this.templateService.save(id === 0 ? null : id, { title: title.trim(), content }).subscribe({
      next: saved => {
        this.message.set(`Texto padrão "${saved.title}" salvo.`);
        this.editingId.set(null);
        this.saving.set(false);
        this.load();
      },
      error: error => {
        this.error.set(toErrorMessage(error));
        this.saving.set(false);
      }
    });
  }

  protected async remove(template: PrescriptionTemplate): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'Excluir texto padrão',
      message: `Excluir o texto padrão "${template.title}"?`,
      confirmText: 'Excluir'
    });
    if (!confirmed) {
      return;
    }
    this.templateService.delete(template.id).subscribe({
      next: () => {
        this.message.set(`Texto padrão "${template.title}" excluído.`);
        this.load();
      },
      error: error => this.error.set(toErrorMessage(error))
    });
  }

  private load(): void {
    this.loading.set(true);
    this.templateService.list().subscribe({
      next: templates => {
        this.templates.set(templates);
        this.loading.set(false);
      },
      error: error => {
        this.error.set(toErrorMessage(error));
        this.loading.set(false);
      }
    });
  }
}
