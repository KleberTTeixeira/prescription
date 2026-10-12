import { DatePipe } from '@angular/common';
import { Component, InjectionToken, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { LoggedProfessionalService } from '../../../application/logged-professional.service';
import { PrescriptionService } from '../../../application/prescription.service';
import { SEX_LABELS, formatAddress } from '../../../domain/models/catalog';
import { PrescriptionPrintData } from '../../../domain/models/prescription';
import { toErrorMessage } from '../../../infrastructure/http/problem-details';
import { ConfirmService } from '../../components/confirm-dialog/confirm-dialog';

/** Opens the browser print dialog (replaceable in the tests). */
export const PRINT_WINDOW = new InjectionToken<() => void>('PRINT_WINDOW', {
  providedIn: 'root',
  factory: () => () => window.print()
});

/**
 * ECF.10.04 – printed prescription. After printing the system asks "A receita foi impressa?";
 * only when confirmed the API issues the prescription (first print; it becomes locked).
 */
@Component({
  selector: 'app-prescription-print',
  imports: [DatePipe, RouterLink],
  templateUrl: './prescription-print.html',
  styleUrl: './prescription-print.css'
})
export class PrescriptionPrint implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly prescriptionService = inject(PrescriptionService);
  private readonly loggedProfessional = inject(LoggedProfessionalService);
  private readonly confirm = inject(ConfirmService);
  private readonly printWindow = inject(PRINT_WINDOW);

  protected readonly sexLabels = SEX_LABELS;
  protected readonly formatAddress = formatAddress;

  protected readonly data = signal<PrescriptionPrintData | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly message = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  protected async print(): Promise<void> {
    const data = this.data();
    if (!data) {
      return;
    }
    this.message.set(null);
    this.printWindow();

    // Already issued, or prescription of another professional (can only reprint): nothing to confirm.
    const canIssue = data.printedAt === null && data.professional.id === this.loggedProfessional.professional()?.id;
    if (!canIssue) {
      return;
    }

    const printed = await this.confirm.ask({
      title: 'Confirmar impressão',
      message: 'A receita foi impressa?',
      confirmText: 'Sim',
      cancelText: 'Não'
    });
    if (!printed) {
      return;
    }

    this.prescriptionService.confirmPrint(data.id).subscribe({
      next: () => {
        this.message.set('Receita emitida. A partir de agora ela não pode mais ser alterada.');
        this.load();
      },
      error: error => this.error.set(toErrorMessage(error))
    });
  }

  private load(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.prescriptionService.printData(id).subscribe({
      next: data => this.data.set(data),
      error: error => this.error.set(toErrorMessage(error))
    });
  }
}
