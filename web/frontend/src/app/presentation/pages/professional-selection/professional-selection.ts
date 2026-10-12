import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { LoggedProfessionalService } from '../../../application/logged-professional.service';
import { ProfessionalService } from '../../../application/professional.service';
import { Professional } from '../../../domain/models/professional';
import { toErrorMessage } from '../../../infrastructure/http/problem-details';

/** Initial screen: the user chooses the professional (in place of a login). */
@Component({
  selector: 'app-professional-selection',
  templateUrl: './professional-selection.html'
})
export class ProfessionalSelection implements OnInit {
  private readonly professionalService = inject(ProfessionalService);
  private readonly loggedProfessional = inject(LoggedProfessionalService);
  private readonly router = inject(Router);

  protected readonly professionals = signal<Professional[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.professionalService.list().subscribe({
      next: professionals => {
        this.professionals.set(professionals);
        this.loading.set(false);
      },
      error: error => {
        this.error.set(toErrorMessage(error));
        this.loading.set(false);
      }
    });
  }

  protected choose(professional: Professional): void {
    this.loggedProfessional.select(professional);
    this.router.navigate(['/receitas']);
  }
}
