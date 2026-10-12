import { Component, Injectable, inject } from '@angular/core';
import { NgbActiveModal, NgbModal } from '@ng-bootstrap/ng-bootstrap';

export interface ConfirmOptions {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
}

/** Confirmation modal (ng-bootstrap; the browser confirm() is not used). */
@Component({
  selector: 'app-confirm-dialog',
  template: `
    <div class="modal-header">
      <h2 class="modal-title h5">{{ options.title }}</h2>
    </div>
    <div class="modal-body">
      <p class="mb-0">{{ options.message }}</p>
    </div>
    <div class="modal-footer">
      <button type="button" class="btn btn-outline-secondary" data-testid="cancel" (click)="modal.close(false)">
        {{ options.cancelText ?? 'Cancelar' }}
      </button>
      <button type="button" class="btn btn-primary" data-testid="confirm" (click)="modal.close(true)">
        {{ options.confirmText ?? 'Confirmar' }}
      </button>
    </div>
  `
})
export class ConfirmDialog {
  protected readonly modal = inject(NgbActiveModal);
  options: ConfirmOptions = { title: '', message: '' };
}

/** Opens the confirmation modal; resolves true only when the user confirms. */
@Injectable({ providedIn: 'root' })
export class ConfirmService {
  private readonly modalService = inject(NgbModal);

  ask(options: ConfirmOptions): Promise<boolean> {
    const reference = this.modalService.open(ConfirmDialog, { backdrop: 'static' });
    (reference.componentInstance as ConfirmDialog).options = options;
    return reference.result.then(
      result => result === true,
      () => false
    );
  }
}
