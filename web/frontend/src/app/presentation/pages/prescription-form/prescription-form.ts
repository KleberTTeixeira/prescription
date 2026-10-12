import { Component, OnInit, inject, signal } from '@angular/core';
import { FormArray, FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { CatalogService, PrescriptionFormLists } from '../../../application/catalog.service';
import { FavoriteService } from '../../../application/favorite.service';
import { LoggedProfessionalService } from '../../../application/logged-professional.service';
import { MedicationService } from '../../../application/medication.service';
import {
  PrescriptionDraft,
  PrescriptionItemDraft,
  catalogMedicationIds,
  toPrescriptionRequest,
  validatePrescription
} from '../../../application/prescription-draft';
import { PrescriptionService } from '../../../application/prescription.service';
import { TemplateService } from '../../../application/template.service';
import { Medication } from '../../../domain/models/catalog';
import {
  CATEGORIES,
  CATEGORY_LABELS,
  Prescription,
  PrescriptionCategory,
  PrescriptionFormat,
  PrescriptionItem
} from '../../../domain/models/prescription';
import { PrescriptionTemplate } from '../../../domain/models/template';
import { toErrorMessage } from '../../../infrastructure/http/problem-details';
import { MedicationSearch } from '../../components/medication-search/medication-search';

type ItemForm = FormGroup<{ [K in keyof PrescriptionItemDraft]: FormControl<PrescriptionItemDraft[K]> }>;

/** Create / edit a prescription (ECF.10.01, ECF.10.02, ECF.10.03, ECF.10.10). */
@Component({
  selector: 'app-prescription-form',
  imports: [ReactiveFormsModule, RouterLink, MedicationSearch],
  templateUrl: './prescription-form.html'
})
export class PrescriptionForm implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly prescriptionService = inject(PrescriptionService);
  private readonly catalogService = inject(CatalogService);
  private readonly templateService = inject(TemplateService);
  private readonly favoriteService = inject(FavoriteService);
  private readonly medicationService = inject(MedicationService);
  private readonly loggedProfessional = inject(LoggedProfessionalService);

  protected readonly categories = CATEGORIES;
  protected readonly categoryLabels = CATEGORY_LABELS;

  protected readonly id = signal<number | null>(null);
  protected readonly lists = signal<PrescriptionFormLists | null>(null);
  protected readonly templates = signal<PrescriptionTemplate[]>([]);
  protected readonly favorites = signal<Medication[]>([]);
  protected readonly suggestedCategory = signal<PrescriptionCategory | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly blocked = signal<string | null>(null);
  protected readonly errors = signal<string[]>([]);
  protected readonly saving = signal(false);
  /** Changed after every form change made outside a template event (zoneless change detection). */
  protected readonly version = signal(0);

  protected readonly templateId = new FormControl<number | null>(null);

  protected readonly form = new FormGroup({
    clientId: new FormControl<number | null>(null),
    institutionId: new FormControl<number | null>(null),
    format: new FormControl<PrescriptionFormat>('Structured', { nonNullable: true }),
    category: new FormControl<PrescriptionCategory>('Common', { nonNullable: true }),
    freeText: new FormControl('', { nonNullable: true }),
    items: new FormArray<ItemForm>([])
  });

  private prescription: Prescription | null = null;

  protected get items(): FormArray<ItemForm> {
    return this.form.controls.items;
  }

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    const id = idParam === null ? null : Number(idParam);
    this.id.set(id);

    forkJoin({
      lists: this.catalogService.formLists(),
      templates: this.templateService.list(),
      favorites: this.favoriteService.list(),
      prescription: id === null ? of(null) : this.prescriptionService.get(id)
    }).subscribe({
      next: result => {
        this.templates.set(result.templates);
        this.favorites.set(result.favorites);
        if (result.prescription) {
          this.load(result.prescription);
        } else {
          this.addItem();
        }
        this.lists.set(result.lists);
      },
      error: error => this.loadError.set(toErrorMessage(error))
    });
  }

  protected formName(id: number | null): string {
    return this.lists()?.pharmaceuticalForms.find(form => form.id === id)?.name ?? '';
  }

  protected changeFormat(): void {
    if (this.form.controls.format.value === 'Structured' && this.items.length === 0) {
      this.addItem();
    }
    this.suggestCategory();
  }

  /** ECF.10.02 – the standard text is copied to the free text, which stays editable. */
  protected useTemplate(): void {
    const template = this.templates().find(t => t.id === this.templateId.value);
    if (template) {
      this.form.controls.freeText.setValue(template.content);
    }
  }

  protected addItem(): void {
    this.items.push(
      new FormGroup({
        medicationId: new FormControl<number | null>(null),
        notRegistered: new FormControl(false, { nonNullable: true }),
        medicationName: new FormControl('', { nonNullable: true }),
        pharmaceuticalFormId: new FormControl<number | null>(null),
        dose: new FormControl<number | null>(null),
        doseUnitId: new FormControl<number | null>(null),
        frequencyId: new FormControl<number | null>(null),
        startDate: new FormControl(this.prescriptionService.issueDate(this.prescription), { nonNullable: true }),
        durationDays: new FormControl<number | null>(null),
        isContinuousUse: new FormControl(false, { nonNullable: true }),
        administrationRouteId: new FormControl<number | null>(null),
        usageDeadline: new FormControl('', { nonNullable: true }),
        notes: new FormControl('', { nonNullable: true })
      })
    );
  }

  protected removeItem(index: number): void {
    this.items.removeAt(index);
    this.suggestCategory();
  }

  /** Catalog medication: name and form from the catalog; usage deadline suggested by the API (ECF.10.03). */
  protected chooseMedication(index: number, medication: Medication): void {
    const item = this.items.at(index);
    item.patchValue({
      medicationId: medication.id,
      notRegistered: false,
      medicationName: medication.displayName,
      pharmaceuticalFormId: medication.pharmaceuticalFormId,
      usageDeadline: ''
    });
    this.medicationService
      .usageDeadline(medication.id, this.prescriptionService.issueDate(this.prescription))
      .subscribe({
        next: deadline => {
          item.controls.usageDeadline.setValue(deadline ?? '');
          this.version.update(v => v + 1);
        },
        error: error => this.errors.set([toErrorMessage(error)])
      });
    this.suggestCategory();
  }

  /** ECF.10.10 – favorite of the logged professional. */
  protected chooseFavorite(index: number, medicationId: string): void {
    const medication = this.favorites().find(favorite => favorite.id === Number(medicationId));
    if (medication) {
      this.chooseMedication(index, medication);
    }
  }

  /** Medication not in the catalog: name and form typed; no suggested deadline. */
  protected markNotRegistered(index: number): void {
    this.items.at(index).patchValue({
      medicationId: null,
      notRegistered: true,
      medicationName: '',
      pharmaceuticalFormId: null,
      usageDeadline: ''
    });
    this.suggestCategory();
  }

  protected changeMedication(index: number): void {
    this.items.at(index).patchValue({
      medicationId: null,
      notRegistered: false,
      medicationName: '',
      pharmaceuticalFormId: null,
      usageDeadline: ''
    });
    this.suggestCategory();
  }

  protected save(): void {
    const draft = this.form.getRawValue() as PrescriptionDraft;
    const errors = validatePrescription(draft);
    this.errors.set(errors);
    if (errors.length > 0) {
      return;
    }

    this.saving.set(true);
    this.prescriptionService.save(this.id(), toPrescriptionRequest(draft)).subscribe({
      next: saved => this.router.navigate(['/receitas', saved.id]),
      error: error => {
        this.errors.set([toErrorMessage(error)]);
        this.saving.set(false);
      }
    });
  }

  /** Suggested by the system (controlled > antimicrobial > common); the user can change it. */
  private suggestCategory(): void {
    const ids = catalogMedicationIds(this.form.getRawValue() as PrescriptionDraft);
    this.prescriptionService.suggestCategory(ids).subscribe({
      next: category => {
        this.suggestedCategory.set(category);
        this.form.controls.category.setValue(category);
        this.version.update(v => v + 1);
      },
      error: error => this.errors.set([toErrorMessage(error)])
    });
  }

  private load(prescription: Prescription): void {
    if (prescription.isIssued) {
      this.blocked.set('Receita já emitida (impressa) não pode ser alterada.');
      return;
    }
    if (prescription.professionalId !== this.loggedProfessional.professional()?.id) {
      this.blocked.set('Receita pertence a outro profissional.');
      return;
    }

    this.prescription = prescription;
    this.form.patchValue({
      clientId: prescription.clientId,
      institutionId: prescription.institutionId,
      format: prescription.format,
      category: prescription.category,
      freeText: prescription.freeText ?? ''
    });
    prescription.items.forEach(item => this.addLoadedItem(item));
    if (prescription.format === 'Structured' && this.items.length === 0) {
      this.addItem();
    }
  }

  private addLoadedItem(item: PrescriptionItem): void {
    this.addItem();
    this.items.at(this.items.length - 1).setValue({
      medicationId: item.medicationId,
      notRegistered: item.medicationId === null,
      medicationName: item.medicationName,
      pharmaceuticalFormId: item.pharmaceuticalFormId,
      dose: item.dose,
      doseUnitId: item.doseUnitId,
      frequencyId: item.frequencyId,
      startDate: item.startDate,
      durationDays: item.durationDays,
      isContinuousUse: item.isContinuousUse,
      administrationRouteId: item.administrationRouteId,
      usageDeadline: item.usageDeadline ?? '',
      notes: item.notes ?? ''
    });
  }
}
