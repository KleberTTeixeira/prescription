import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { LoggedProfessionalService } from '../../application/logged-professional.service';

export const PROFESSIONAL_ID_HEADER = 'X-Professional-Id';

/** Sends the chosen professional's id on every request to the API (/api and /odata). */
export const professionalIdInterceptor: HttpInterceptorFn = (request, next) => {
  const professional = inject(LoggedProfessionalService).professional();
  const isApi = request.url.startsWith('/api/') || request.url.startsWith('/odata/');

  if (!professional || !isApi) {
    return next(request);
  }

  return next(request.clone({ setHeaders: { [PROFESSIONAL_ID_HEADER]: String(professional.id) } }));
};
