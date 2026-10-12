import { HttpErrorResponse } from '@angular/common/http';

/**
 * Message to show the user for an error. The API answers errors as ProblemDetails (RFC 7807)
 * with the reason in Portuguese in "detail".
 */
export function toErrorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    const detail: unknown = error.error?.detail;
    if (typeof detail === 'string' && detail.trim() !== '') {
      return detail;
    }
    if (error.status === 0) {
      return 'Não foi possível conectar ao servidor.';
    }
  }
  return 'Ocorreu um erro inesperado.';
}
