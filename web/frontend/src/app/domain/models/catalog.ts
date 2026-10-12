/** Support data (pre-populated in the database). */

export type Sex = 'Male' | 'Female';

export const SEX_LABELS: Record<Sex, string> = { Male: 'Masculino', Female: 'Feminino' };

export interface Address {
  street: string;
  number: string;
  complement: string | null;
  district: string;
  city: string;
  state: string;
  zipCode: string;
}

export interface Client {
  id: number;
  fullName: string;
  /** yyyy-MM-dd */
  birthDate: string;
  sex: Sex;
  address: Address;
}

export interface Institution {
  id: number;
  name: string;
  cnes: string;
  address: Address;
  phone: string;
}

/** Catalog medication. displayName = name + concentration (stored in the prescription item). */
export interface Medication {
  id: number;
  name: string;
  activeIngredient: string;
  concentration: string | null;
  displayName: string;
  pharmaceuticalFormId: number;
  pharmaceuticalFormName: string;
  /** Days of the validity period (null = no limit). */
  validityPeriodDays: number | null;
  isControlled: boolean;
  isAntimicrobial: boolean;
}

/** Pharmaceutical forms, administration routes and frequencies. */
export interface Lookup {
  id: number;
  name: string;
}

export interface DoseUnit {
  id: number;
  abbreviation: string;
  name: string;
}

export function formatAddress(address: Address): string {
  const complement = address.complement ? ` – ${address.complement}` : '';
  const zipCode = `${address.zipCode.slice(0, 5)}-${address.zipCode.slice(5)}`;
  return `${address.street}, ${address.number}${complement}, ${address.district}, ${address.city}/${address.state}, CEP ${zipCode}`;
}
