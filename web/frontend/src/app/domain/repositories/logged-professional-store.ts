import { Professional } from '../models/professional';

/** Contract for keeping the chosen ("logged") professional (implemented in infrastructure). */
export abstract class LoggedProfessionalStore {
  abstract get(): Professional | null;
  abstract set(professional: Professional): void;
  abstract clear(): void;
}
