import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { LoggedProfessionalService } from '../../../application/logged-professional.service';
import { PrescriptionService } from '../../../application/prescription.service';
import { CATEGORY_LABELS, FORMAT_LABELS, Prescription } from '../../../domain/models/prescription';
import { toErrorMessage } from '../../../infrastructure/http/problem-details';
import { ConfirmService } from '../../components/confirm-dialog/confirm-dialog';

/** Prescription details (any professional can view and reprint; only the owner edits and deletes). */
@Component({
  selector: 'app-prescription-view',
  imports: [DatePipe, RouterLink],
  templateUrl: './prescription-view.html'
})
export class PrescriptionView implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly prescriptionService = inject(PrescriptionService);
  private readonly loggedProfessional = inject(LoggedProfessionalService);
  private readonly confirm = inject(ConfirmService);

  protected readonly formatLabels = FORMAT_LABELS;
  protected readonly categoryLabels = CATEGORY_LABELS;

  protected readonly prescription = signal<Prescription | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly isOwner = computed(
    () => this.prescription()?.professionalId === this.loggedProfessional.professional()?.id
  );

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.prescriptionService.get(id).subscribe({
      next: prescription => this.prescription.set(prescription),
      error: error => this.error.set(toErrorMessage(error))
    });
  }

  protected async remove(prescription: Prescription): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'Excluir receita',
      message: `Excluir a receita nº ${prescription.id} de ${prescription.clientName}?`,
      confirmText: 'Excluir'
    });
    if (!confirmed) {
      return;
    }
    this.prescriptionService.delete(prescription.id).subscribe({
      next: () => this.router.navigate(['/receitas']),
      error: error => this.error.set(toErrorMessage(error))
    });
  }
}
