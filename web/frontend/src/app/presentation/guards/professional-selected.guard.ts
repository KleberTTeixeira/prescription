import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { LoggedProfessionalService } from '../../application/logged-professional.service';

/** Without a chosen professional, sends the user to the selection screen. */
export const professionalSelectedGuard: CanActivateFn = () =>
  inject(LoggedProfessionalService).professional() !== null ||
  inject(Router).createUrlTree(['/profissional']);
