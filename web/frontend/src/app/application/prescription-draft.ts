import {
  PrescriptionCategory,
  PrescriptionFormat,
  PrescriptionItemRequest,
  PrescriptionRequest
} from '../domain/models/prescription';

/** Item as filled on the screen (empty text = '', not chosen = null). */
export interface PrescriptionItemDraft {
  /** Catalog medication (name and form come from the catalog). */
  medicationId: number | null;
  /** Medication not in the catalog (e.g. compounded): name and form typed. */
  notRegistered: boolean;
  medicationName: string;
  pharmaceuticalFormId: number | null;
  dose: number | null;
  doseUnitId: number | null;
  frequencyId: number | null;
  startDate: string;
  durationDays: number | null;
  isContinuousUse: boolean;
  administrationRouteId: number | null;
  usageDeadline: string;
  notes: string;
}

export interface PrescriptionDraft {
  clientId: number | null;
  institutionId: number | null;
  format: PrescriptionFormat;
  category: PrescriptionCategory;
  freeText: string;
  items: PrescriptionItemDraft[];
}

const required = (field: string) => `Campo '${field}' é obrigatório.`;
const greaterThanZero = (field: string) => `Campo '${field}' deve ser maior que zero.`;

/** Same rules and messages as the Domain; the API checks them again. */
export function validatePrescription(draft: PrescriptionDraft): string[] {
  const errors: string[] = [];
  if (!draft.clientId) {
    errors.push(required('Cliente'));
  }
  if (!draft.institutionId) {
    errors.push(required('Instituição'));
  }

  if (draft.format === 'Unstructured') {
    if (draft.freeText.trim() === '') {
      errors.push('Receita não estruturada exige texto livre.');
    }
    return errors;
  }

  if (draft.items.length === 0) {
    errors.push('Receita estruturada exige no mínimo 1 item.');
  }
  draft.items.forEach((item, index) => {
    const prefix = `Item ${index + 1}: `;
    if (item.notRegistered) {
      if (item.medicationName.trim() === '') {
        errors.push(prefix + required('Medicamento'));
      }
      if (!item.pharmaceuticalFormId) {
        errors.push(prefix + required('Forma farmacêutica'));
      }
    } else if (!item.medicationId) {
      errors.push(prefix + required('Medicamento'));
    }
    if (item.dose === null || item.dose <= 0) {
      errors.push(prefix + greaterThanZero('Dose'));
    }
    if (!item.doseUnitId) {
      errors.push(prefix + required('Unidade'));
    }
    if (!item.frequencyId) {
      errors.push(prefix + required('Frequência'));
    }
    if (item.startDate === '') {
      errors.push(prefix + required('Início'));
    }
    if (item.durationDays !== null && item.durationDays <= 0) {
      errors.push(prefix + greaterThanZero('Duração (dias)'));
    }
    if (!item.administrationRouteId) {
      errors.push(prefix + required('Via de administração'));
    }
  });
  return errors;
}

/** Optional text with only spaces is sent as empty (null). */
function optionalText(value: string): string | null {
  const trimmed = value.trim();
  return trimmed === '' ? null : trimmed;
}

function toItemRequest(item: PrescriptionItemDraft): PrescriptionItemRequest {
  const registered = !item.notRegistered && item.medicationId !== null;
  return {
    medicationId: registered ? item.medicationId : null,
    medicationName: registered ? null : optionalText(item.medicationName),
    pharmaceuticalFormId: registered ? null : item.pharmaceuticalFormId,
    dose: item.dose ?? 0,
    doseUnitId: item.doseUnitId ?? 0,
    frequencyId: item.frequencyId ?? 0,
    startDate: item.startDate,
    durationDays: item.durationDays,
    isContinuousUse: item.isContinuousUse,
    administrationRouteId: item.administrationRouteId ?? 0,
    usageDeadline: item.usageDeadline === '' ? null : item.usageDeadline,
    notes: optionalText(item.notes)
  };
}

/** Request of the API. Unstructured: no items (ECF.10.01). */
export function toPrescriptionRequest(draft: PrescriptionDraft): PrescriptionRequest {
  return {
    clientId: draft.clientId ?? 0,
    institutionId: draft.institutionId ?? 0,
    format: draft.format,
    category: draft.category,
    freeText: optionalText(draft.freeText),
    items: draft.format === 'Unstructured' ? [] : draft.items.map(toItemRequest)
  };
}

/** Catalog medication ids of the items (base of the suggested category). */
export function catalogMedicationIds(draft: PrescriptionDraft): number[] {
  if (draft.format === 'Unstructured') {
    return [];
  }
  const ids = draft.items
    .filter(item => !item.notRegistered && item.medicationId !== null)
    .map(item => item.medicationId as number);
  return [...new Set(ids)];
}
