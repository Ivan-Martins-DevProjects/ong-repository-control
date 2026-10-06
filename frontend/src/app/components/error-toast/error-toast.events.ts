export const ERROR_TOAST_EVENT = 'app:error-toast';

export interface ErrorToastDetail {
  title: string;
  description: string;
}

export function emitErrorToast(title: string, description: string): void {
  document.dispatchEvent(
    new CustomEvent<ErrorToastDetail>(ERROR_TOAST_EVENT, {
      detail: { title, description },
      bubbles: true,
    })
  );
}
