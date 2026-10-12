import { Injectable } from '@angular/core';
import { Professional } from '../../domain/models/professional';
import { LoggedProfessionalStore } from '../../domain/repositories/logged-professional-store';

export const LOGGED_PROFESSIONAL_KEY = 'prescription.loggedProfessional';

/** Keeps the chosen professional in sessionStorage (valid while the tab is open). */
@Injectable()
export class SessionLoggedProfessionalStore extends LoggedProfessionalStore {
  get(): Professional | null {
    const value = sessionStorage.getItem(LOGGED_PROFESSIONAL_KEY);
    if (!value) {
      return null;
    }
    try {
      return JSON.parse(value) as Professional;
    } catch {
      sessionStorage.removeItem(LOGGED_PROFESSIONAL_KEY);
      return null;
    }
  }

  set(professional: Professional): void {
    sessionStorage.setItem(LOGGED_PROFESSIONAL_KEY, JSON.stringify(professional));
  }

  clear(): void {
    sessionStorage.removeItem(LOGGED_PROFESSIONAL_KEY);
  }
}
