import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { CatalogRepository } from './domain/repositories/catalog-repository';
import { FavoriteRepository } from './domain/repositories/favorite-repository';
import { LoggedProfessionalStore } from './domain/repositories/logged-professional-store';
import { MedicationRepository } from './domain/repositories/medication-repository';
import { PrescriptionRepository } from './domain/repositories/prescription-repository';
import { ProfessionalRepository } from './domain/repositories/professional-repository';
import { TemplateRepository } from './domain/repositories/template-repository';
import { CatalogHttpRepository } from './infrastructure/http/catalog-http-repository';
import { FavoriteHttpRepository } from './infrastructure/http/favorite-http-repository';
import { MedicationHttpRepository } from './infrastructure/http/medication-http-repository';
import { PrescriptionHttpRepository } from './infrastructure/http/prescription-http-repository';
import { ProfessionalHttpRepository } from './infrastructure/http/professional-http-repository';
import { TemplateHttpRepository } from './infrastructure/http/template-http-repository';
import { professionalIdInterceptor } from './infrastructure/http/professional-id.interceptor';
import { SessionLoggedProfessionalStore } from './infrastructure/storage/session-logged-professional-store';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([professionalIdInterceptor])),
    { provide: ProfessionalRepository, useClass: ProfessionalHttpRepository },
    { provide: CatalogRepository, useClass: CatalogHttpRepository },
    { provide: MedicationRepository, useClass: MedicationHttpRepository },
    { provide: PrescriptionRepository, useClass: PrescriptionHttpRepository },
    { provide: TemplateRepository, useClass: TemplateHttpRepository },
    { provide: FavoriteRepository, useClass: FavoriteHttpRepository },
    { provide: LoggedProfessionalStore, useClass: SessionLoggedProfessionalStore }
  ]
};
