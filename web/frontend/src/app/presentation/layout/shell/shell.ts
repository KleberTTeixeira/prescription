import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { NgbCollapse } from '@ng-bootstrap/ng-bootstrap';
import { LoggedProfessionalService } from '../../../application/logged-professional.service';

/** Top bar with the menu and the chosen professional; pages open below it. */
@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, NgbCollapse],
  templateUrl: './shell.html'
})
export class Shell {
  private readonly loggedProfessional = inject(LoggedProfessionalService);
  private readonly router = inject(Router);

  protected readonly professional = this.loggedProfessional.professional;
  protected readonly menuCollapsed = signal(true);

  protected changeProfessional(): void {
    this.loggedProfessional.clear();
    this.router.navigate(['/profissional']);
  }
}
