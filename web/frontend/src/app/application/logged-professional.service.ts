import { Injectable, inject, signal } from '@angular/core';
import { Professional } from '../domain/models/professional';
import { LoggedProfessionalStore } from '../domain/repositories/logged-professional-store';

/** Professional chosen on the initial screen (in place of a login). */
@Injectable({ providedIn: 'root' })
export class LoggedProfessionalService {
  private readonly store = inject(LoggedProfessionalStore);
  private readonly current = signal<Professional | null>(this.store.get());

  readonly professional = this.current.asReadonly();

  select(professional: Professional): void {
    this.store.set(professional);
    this.current.set(professional);
  }

  clear(): void {
    this.store.clear();
    this.current.set(null);
  }
}
