import {
  PrescriptionDraft,
  PrescriptionItemDraft,
  catalogMedicationIds,
  toPrescriptionRequest,
  validatePrescription
} from './prescription-draft';

function item(overrides: Partial<PrescriptionItemDraft> = {}): PrescriptionItemDraft {
  return {
    medicationId: 5,
    notRegistered: false,
    medicationName: 'Amoxicilina 500 mg',
    pharmaceuticalFormId: 2,
    dose: 1,
    doseUnitId: 7,
    frequencyId: 3,
    startDate: '2026-10-12',
    durationDays: 7,
    isContinuousUse: false,
    administrationRouteId: 1,
    usageDeadline: '2026-10-22',
    notes: '',
    ...overrides
  };
}

function draft(overrides: Partial<PrescriptionDraft> = {}): PrescriptionDraft {
  return {
    clientId: 1,
    institutionId: 1,
    format: 'Structured',
    category: 'Antimicrobial',
    freeText: '',
    items: [item()],
    ...overrides
  };
}

describe('validatePrescription', () => {
  it('accepts a valid structured prescription', () => {
    expect(validatePrescription(draft())).toEqual([]);
  });

  it('requires client and institution', () => {
    expect(validatePrescription(draft({ clientId: null, institutionId: null }))).toEqual([
      "Campo 'Cliente' é obrigatório.",
      "Campo 'Instituição' é obrigatório."
    ]);
  });

  it('ECF.10.01 – unstructured requires free text (only spaces = empty)', () => {
    expect(validatePrescription(draft({ format: 'Unstructured', freeText: '   ' }))).toEqual([
      'Receita não estruturada exige texto livre.'
    ]);
    expect(validatePrescription(draft({ format: 'Unstructured', freeText: 'Repouso.' }))).toEqual([]);
  });

  it('structured requires at least 1 item', () => {
    expect(validatePrescription(draft({ items: [] }))).toEqual(['Receita estruturada exige no mínimo 1 item.']);
  });

  it('item requires medication, dose > 0, unit, frequency, start, route; duration > 0 when filled', () => {
    const errors = validatePrescription(
      draft({
        items: [
          item({
            medicationId: null,
            dose: 0,
            doseUnitId: null,
            frequencyId: null,
            startDate: '',
            durationDays: 0,
            administrationRouteId: null
          })
        ]
      })
    );
    expect(errors).toEqual([
      "Item 1: Campo 'Medicamento' é obrigatório.",
      "Item 1: Campo 'Dose' deve ser maior que zero.",
      "Item 1: Campo 'Unidade' é obrigatório.",
      "Item 1: Campo 'Frequência' é obrigatório.",
      "Item 1: Campo 'Início' é obrigatório.",
      "Item 1: Campo 'Duração (dias)' deve ser maior que zero.",
      "Item 1: Campo 'Via de administração' é obrigatório."
    ]);
  });

  it('ECF.10.03 – not registered medication requires name and form', () => {
    const errors = validatePrescription(
      draft({ items: [item({ medicationId: null, notRegistered: true, medicationName: ' ', pharmaceuticalFormId: null })] })
    );
    expect(errors).toEqual([
      "Item 1: Campo 'Medicamento' é obrigatório.",
      "Item 1: Campo 'Forma farmacêutica' é obrigatório."
    ]);
  });

  it('usage deadline is optional (no validation)', () => {
    expect(validatePrescription(draft({ items: [item({ usageDeadline: '' })] }))).toEqual([]);
  });
});

describe('toPrescriptionRequest', () => {
  it('catalog medication: sends only the id (name and form come from the catalog)', () => {
    const request = toPrescriptionRequest(draft({ items: [item({ notes: '  ' })] }));
    expect(request.items[0]).toEqual({
      medicationId: 5,
      medicationName: null,
      pharmaceuticalFormId: null,
      dose: 1,
      doseUnitId: 7,
      frequencyId: 3,
      startDate: '2026-10-12',
      durationDays: 7,
      isContinuousUse: false,
      administrationRouteId: 1,
      usageDeadline: '2026-10-22',
      notes: null
    });
    expect(request.freeText).toBeNull();
  });

  it('not registered medication: sends typed name and form; empty deadline = null', () => {
    const request = toPrescriptionRequest(
      draft({
        items: [
          item({ medicationId: null, notRegistered: true, medicationName: ' Fórmula X ', pharmaceuticalFormId: 4, usageDeadline: '' })
        ]
      })
    );
    expect(request.items[0].medicationId).toBeNull();
    expect(request.items[0].medicationName).toBe('Fórmula X');
    expect(request.items[0].pharmaceuticalFormId).toBe(4);
    expect(request.items[0].usageDeadline).toBeNull();
  });

  it('unstructured: no items', () => {
    const request = toPrescriptionRequest(draft({ format: 'Unstructured', freeText: 'Repouso.' }));
    expect(request.items).toEqual([]);
    expect(request.freeText).toBe('Repouso.');
  });
});

describe('catalogMedicationIds', () => {
  it('distinct catalog ids; ignores not registered and unstructured', () => {
    const items = [item({ medicationId: 5 }), item({ medicationId: 5 }), item({ medicationId: null, notRegistered: true })];
    expect(catalogMedicationIds(draft({ items }))).toEqual([5]);
    expect(catalogMedicationIds(draft({ format: 'Unstructured', items }))).toEqual([]);
  });
});
