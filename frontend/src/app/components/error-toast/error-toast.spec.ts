import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ErrorToast } from './error-toast';
import { emitErrorToast } from './error-toast.events';

describe('ErrorToast', () => {
  let fixture: ComponentFixture<ErrorToast>;
  let element: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ErrorToast],
    }).compileComponents();

    fixture = TestBed.createComponent(ErrorToast);
    element = fixture.nativeElement;
    fixture.detectChanges();
  });

  it('deve criar o componente', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('deve exibir título e descrição quando o listener é acionado', () => {
    emitErrorToast('Falha ao salvar', 'Não foi possível conectar ao servidor');
    fixture.detectChanges();

    const toast = element.querySelector('.error-toast');
    expect(toast?.classList.contains('error-toast--visible')).toBe(true);
    expect(element.querySelector('.error-toast__title')?.textContent?.trim()).toBe('Falha ao salvar');
    expect(element.querySelector('.error-toast__description')?.textContent?.trim()).toBe(
      'Não foi possível conectar ao servidor'
    );
  });

  it('deve exibir um novo erro quando acionado novamente', () => {
    emitErrorToast('Primeiro erro', 'Descrição 1');
    fixture.detectChanges();
    emitErrorToast('Segundo erro', 'Descrição 2');
    fixture.detectChanges();

    expect(element.querySelector('.error-toast__title')?.textContent?.trim()).toBe('Segundo erro');
    expect(element.querySelector('.error-toast__description')?.textContent?.trim()).toBe('Descrição 2');
  });

  it('deve ocultar o toast ao clicar no botão de fechar', () => {
    emitErrorToast('Erro', 'Descrição');
    fixture.detectChanges();

    const closeButton = element.querySelector<HTMLButtonElement>('.error-toast__close');
    closeButton?.click();
    fixture.detectChanges();

    const toast = element.querySelector('.error-toast');
    expect(toast?.classList.contains('error-toast--visible')).toBe(false);
  });

  it('deve exibir o backdrop quando o toast aparece e removê-lo ao fechar', () => {
    const backdrop = element.querySelector('.error-toast__backdrop');
    expect(backdrop?.classList.contains('error-toast__backdrop--visible')).toBe(false);

    emitErrorToast('Erro', 'Descrição');
    fixture.detectChanges();
    expect(backdrop?.classList.contains('error-toast__backdrop--visible')).toBe(true);

    element.querySelector<HTMLButtonElement>('.error-toast__close')?.click();
    fixture.detectChanges();
    expect(backdrop?.classList.contains('error-toast__backdrop--visible')).toBe(false);
  });

  it('deve parar de escutar o evento após o destroy', () => {
    fixture.destroy();

    expect(() => emitErrorToast('Erro', 'Descrição')).not.toThrow();
  });
});
