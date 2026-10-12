import { Component, inject, input, output, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MedicationService } from '../../../application/medication.service';
import { Medication } from '../../../domain/models/catalog';
import { toErrorMessage } from '../../../infrastructure/http/problem-details';

/**
 * ECF.10.03 b – search of a catalog medication by name or active ingredient.
 * ECF.10.07 – result list with clearly distinct rows, highlighted on mouse over and when chosen.
 */
@Component({
  selector: 'app-medication-search',
  imports: [ReactiveFormsModule],
  templateUrl: './medication-search.html',
  styleUrl: './medication-search.css'
})
export class MedicationSearch {
  private readonly medicationService = inject(MedicationService);

  readonly actionText = input('Escolher');
  readonly selected = output<Medication>();

  protected readonly term = new FormControl('', { nonNullable: true });
  protected readonly results = signal<Medication[]>([]);
  protected readonly searched = signal(false);
  protected readonly searching = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly chosenId = signal<number | null>(null);

  protected search(): void {
    this.error.set(null);
    this.chosenId.set(null);
    this.searching.set(true);
    this.medicationService.search(this.term.value).subscribe({
      next: medications => {
        this.results.set(medications);
        this.searched.set(this.term.value.trim() !== '');
        this.searching.set(false);
      },
      error: error => {
        this.error.set(toErrorMessage(error));
        this.searching.set(false);
      }
    });
  }

  protected choose(medication: Medication): void {
    this.chosenId.set(medication.id);
    this.selected.emit(medication);
  }
}
