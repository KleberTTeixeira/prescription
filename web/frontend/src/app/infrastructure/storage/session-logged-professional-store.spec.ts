import { Professional } from '../../domain/models/professional';
import { LOGGED_PROFESSIONAL_KEY, SessionLoggedProfessionalStore } from './session-logged-professional-store';

describe('SessionLoggedProfessionalStore', () => {
  const professional: Professional = {
    id: 3, fullName: 'Bruno Lima', council: 'CRM', councilNumber: '54321', councilState: 'RJ'
  };
  let store: SessionLoggedProfessionalStore;

  beforeEach(() => {
    sessionStorage.clear();
    store = new SessionLoggedProfessionalStore();
  });

  it('returns null when nothing was saved', () => {
    expect(store.get()).toBeNull();
  });

  it('saves and reads the professional in sessionStorage', () => {
    store.set(professional);
    expect(sessionStorage.getItem(LOGGED_PROFESSIONAL_KEY)).not.toBeNull();
    expect(store.get()).toEqual(professional);
  });

  it('clears the saved professional', () => {
    store.set(professional);
    store.clear();
    expect(store.get()).toBeNull();
  });

  it('discards an invalid saved value', () => {
    sessionStorage.setItem(LOGGED_PROFESSIONAL_KEY, '{invalid');
    expect(store.get()).toBeNull();
    expect(sessionStorage.getItem(LOGGED_PROFESSIONAL_KEY)).toBeNull();
  });
});
