import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { ERROR_TOAST_EVENT, type ErrorToastDetail } from './error-toast.events';

const VISIBLE_MS = 6000;

@Component({
  selector: 'app-error-toast',
  imports: [],
  templateUrl: './error-toast.html',
  styleUrl: './error-toast.css',
})
export class ErrorToast implements OnInit, OnDestroy {
  protected readonly visible = signal(false);
  protected readonly title = signal('');
  protected readonly description = signal('');

  private hideTimeout?: ReturnType<typeof setTimeout>;

  private readonly onShowError = (event: Event) => {
    const detail = (event as CustomEvent<ErrorToastDetail>).detail;
    if (!detail) return;

    this.title.set(detail.title);
    this.description.set(detail.description);
    this.visible.set(true);
    this.scheduleHide();
  };

  ngOnInit(): void {
    window.addEventListener(ERROR_TOAST_EVENT, this.onShowError);
  }

  ngOnDestroy(): void {
    window.removeEventListener(ERROR_TOAST_EVENT, this.onShowError);
    this.clearHideTimeout();
  }

  protected dismiss(): void {
    this.visible.set(false);
    this.clearHideTimeout();
  }

  private scheduleHide(): void {
    this.clearHideTimeout();
    this.hideTimeout = setTimeout(() => this.visible.set(false), VISIBLE_MS);
  }

  private clearHideTimeout(): void {
    if (this.hideTimeout) {
      clearTimeout(this.hideTimeout);
      this.hideTimeout = undefined;
    }
  }
}
