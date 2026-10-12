import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { NgbPagination } from '@ng-bootstrap/ng-bootstrap';
import { PrescriptionService } from '../../../application/prescription.service';
import {
  CATEGORY_LABELS,
  FORMAT_LABELS,
  PrescriptionListItem,
  PrescriptionSort,
  PrescriptionSortField
} from '../../../domain/models/prescription';
import { toErrorMessage } from '../../../infrastructure/http/problem-details';
import { ConfirmService } from '../../components/confirm-dialog/confirm-dialog';

/** Prescriptions of the logged professional (OData), most recent first. */
@Component({
  selector: 'app-prescriptions',
  imports: [DatePipe, ReactiveFormsModule, RouterLink, NgbPagination],
  templateUrl: './prescriptions.html'
})
export class Prescriptions implements OnInit {
  private readonly prescriptionService = inject(PrescriptionService);
  private readonly confirm = inject(ConfirmService);

  protected readonly formatLabels = FORMAT_LABELS;
  protected readonly categoryLabels = CATEGORY_LABELS;
  protected readonly pageSize = PrescriptionService.PAGE_SIZE;
  protected readonly columns: { field: PrescriptionSortField; title: string }[] = [
    { field: 'id', title: 'Nº' },
    { field: 'issuedAt', title: 'Data' },
    { field: 'clientName', title: 'Cliente' },
    { field: 'format', title: 'Formato' },
    { field: 'category', title: 'Categoria' },
    { field: 'itemCount', title: 'Itens' },
    { field: 'isIssued', title: 'Situação' }
  ];

  protected readonly clientName = new FormControl('', { nonNullable: true });
  protected readonly rows = signal<PrescriptionListItem[]>([]);
  protected readonly total = signal(0);
  protected readonly page = signal(1);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly message = signal<string | null>(null);
  /** Default: most recent first. */
  protected readonly sort = signal<PrescriptionSort>({ field: 'issuedAt', direction: 'desc' });

  private filter = '';

  ngOnInit(): void {
    this.load();
  }

  protected search(): void {
    this.filter = this.clientName.value;
    this.page.set(1);
    this.load();
  }

  /** Click on a column title: sorts by it; a second click inverts the direction. */
  protected sortBy(field: PrescriptionSortField): void {
    const current = this.sort();
    const direction = current.field === field && current.direction === 'asc' ? 'desc' : 'asc';
    this.sort.set({ field, direction });
    this.page.set(1);
    this.load();
  }

  protected sortIndicator(field: PrescriptionSortField): string {
    const current = this.sort();
    return current.field !== field ? '' : current.direction === 'asc' ? '▲' : '▼';
  }

  protected ariaSort(field: PrescriptionSortField): string {
    const current = this.sort();
    return current.field !== field ? 'none' : current.direction === 'asc' ? 'ascending' : 'descending';
  }

  protected changePage(page: number): void {
    this.page.set(page);
    this.load();
  }

  protected async remove(row: PrescriptionListItem): Promise<void> {
    const confirmed = await this.confirm.ask({
      title: 'Excluir receita',
      message: `Excluir a receita nº ${row.id} de ${row.clientName}?`,
      confirmText: 'Excluir'
    });
    if (!confirmed) {
      return;
    }
    this.prescriptionService.delete(row.id).subscribe({
      next: () => {
        this.message.set(`Receita nº ${row.id} excluída.`);
        this.load();
      },
      error: error => this.error.set(toErrorMessage(error))
    });
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.prescriptionService
      .list({
        clientName: this.filter,
        sort: this.sort(),
        skip: (this.page() - 1) * this.pageSize,
        top: this.pageSize
      })
      .subscribe({
        next: page => {
          this.rows.set(page.rows);
          this.total.set(page.total);
          this.loading.set(false);
        },
        error: error => {
          this.error.set(toErrorMessage(error));
          this.loading.set(false);
        }
      });
  }
}
