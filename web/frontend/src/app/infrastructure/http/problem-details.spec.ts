import { HttpErrorResponse } from '@angular/common/http';
import { toErrorMessage } from './problem-details';

describe('toErrorMessage', () => {
  it('returns the ProblemDetails detail', () => {
    const error = new HttpErrorResponse({
      status: 400,
      error: { title: 'Requisição inválida', status: 400, detail: 'Cliente 99 não encontrado.' }
    });
    expect(toErrorMessage(error)).toBe('Cliente 99 não encontrado.');
  });

  it('returns a connection message when the server cannot be reached', () => {
    expect(toErrorMessage(new HttpErrorResponse({ status: 0 }))).toBe('Não foi possível conectar ao servidor.');
  });

  it('returns a generic message when there is no detail', () => {
    expect(toErrorMessage(new HttpErrorResponse({ status: 500, error: { detail: '  ' } })))
      .toBe('Ocorreu um erro inesperado.');
    expect(toErrorMessage(new Error('x'))).toBe('Ocorreu um erro inesperado.');
  });
});
