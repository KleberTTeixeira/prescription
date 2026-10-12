import { Address, Institution, Sex } from './catalog';
import { Professional } from './professional';

/** ECF.10.01 Unstructured (free text) / ECF.10.03 Structured (items). */
export type PrescriptionFormat = 'Unstructured' | 'Structured';

export type PrescriptionCategory = 'Common' | 'SpecialControl' | 'Antimicrobial';

export const FORMAT_LABELS: Record<PrescriptionFormat, string> = {
  Unstructured: 'Não estruturada',
  Structured: 'Estruturada'
};

export const CATEGORY_LABELS: Record<PrescriptionCategory, string> = {
  Common: 'Comum',
  SpecialControl: 'Controle especial',
  Antimicrobial: 'Antimicrobiano'
};

export const CATEGORIES: PrescriptionCategory[] = ['Common', 'SpecialControl', 'Antimicrobial'];

/** Dates as text: DATE = yyyy-MM-dd; DATETIME = yyyy-MM-ddTHH:mm:ss (Brasília time). */
export interface PrescriptionItem {
  id: number;
  medicationId: number | null;
  medicationName: string;
  pharmaceuticalFormId: number;
  pharmaceuticalFormName: string;
  dose: number;
  doseUnitId: number;
  doseUnitAbbreviation: string;
  frequencyId: number;
  frequencyName: string;
  startDate: string;
  durationDays: number | null;
  isContinuousUse: boolean;
  administrationRouteId: number;
  administrationRouteName: string;
  usageDeadline: string | null;
  notes: string | null;
}

export interface Prescription {
  id: number;
  clientId: number;
  clientName: string;
  professionalId: number;
  professionalName: string;
  institutionId: number;
  institutionName: string;
  format: PrescriptionFormat;
  category: PrescriptionCategory;
  freeText: string | null;
  issuedAt: string;
  printedAt: string | null;
  /** Printed for the first time (confirmed): locked, only view, reprint and delete. */
  isIssued: boolean;
  items: PrescriptionItem[];
}

/** Row of the prescription list. */
export interface PrescriptionListItem {
  id: number;
  clientName: string;
  institutionName: string;
  format: PrescriptionFormat;
  category: PrescriptionCategory;
  issuedAt: string;
  isIssued: boolean;
  itemCount: number;
}

/** Columns of the prescription list that can be sorted. */
export type PrescriptionSortField = 'id' | 'issuedAt' | 'clientName' | 'format' | 'category' | 'itemCount' | 'isIssued';

export interface PrescriptionSort {
  field: PrescriptionSortField;
  direction: 'asc' | 'desc';
}

export interface PrescriptionListQuery {
  clientName: string;
  sort: PrescriptionSort;
  skip: number;
  top: number;
}

export interface PrescriptionPage {
  rows: PrescriptionListItem[];
  total: number;
}

/** Catalog medication: medicationId filled (name and form come from the catalog); otherwise name and form typed. */
export interface PrescriptionItemRequest {
  medicationId: number | null;
  medicationName: string | null;
  pharmaceuticalFormId: number | null;
  dose: number;
  doseUnitId: number;
  frequencyId: number;
  startDate: string;
  durationDays: number | null;
  isContinuousUse: boolean;
  administrationRouteId: number;
  usageDeadline: string | null;
  notes: string | null;
}

export interface PrescriptionRequest {
  clientId: number;
  institutionId: number;
  format: PrescriptionFormat;
  category: PrescriptionCategory;
  freeText: string | null;
  items: PrescriptionItemRequest[];
}

/** ECF.10.04 – address only for special control; age and sex only for antimicrobial. */
export interface PrintClient {
  fullName: string;
  address: Address | null;
  age: number | null;
  sex: Sex | null;
}

export interface PrescriptionPrintData {
  id: number;
  format: PrescriptionFormat;
  category: PrescriptionCategory;
  freeText: string | null;
  issuedAt: string;
  printedAt: string | null;
  client: PrintClient;
  professional: Professional;
  institution: Institution;
  items: PrescriptionItem[];
}
