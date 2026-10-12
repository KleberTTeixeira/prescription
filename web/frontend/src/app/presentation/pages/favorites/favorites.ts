import { Component, OnInit, inject, signal } from '@angular/core';
import { FavoriteService } from '../../../application/favorite.service';
import { Medication } from '../../../domain/models/catalog';
import { toErrorMessage } from '../../../infrastructure/http/problem-details';
import { MedicationSearch } from '../../components/medication-search/medication-search';

/** ECF.10.10 – favorite medications of the logged professional (used in the prescription). */
@Component({
  selector: 'app-favorites',
  imports: [MedicationSearch],
  templateUrl: './favorites.html'
})
export class Favorites implements OnInit {
  private readonly favoriteService = inject(FavoriteService);

  protected readonly favorites = signal<Medication[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly message = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  protected add(medication: Medication): void {
    this.error.set(null);
    this.favoriteService.add(medication.id).subscribe({
      next: () => {
        this.message.set(`${medication.displayName} adicionado aos favoritos.`);
        this.load();
      },
      error: error => this.error.set(toErrorMessage(error))
    });
  }

  protected remove(medication: Medication): void {
    this.error.set(null);
    this.favoriteService.remove(medication.id).subscribe({
      next: () => {
        this.message.set(`${medication.displayName} removido dos favoritos.`);
        this.load();
      },
      error: error => this.error.set(toErrorMessage(error))
    });
  }

  private load(): void {
    this.favoriteService.list().subscribe({
      next: favorites => {
        this.favorites.set(favorites);
        this.loading.set(false);
      },
      error: error => {
        this.error.set(toErrorMessage(error));
        this.loading.set(false);
      }
    });
  }
}
