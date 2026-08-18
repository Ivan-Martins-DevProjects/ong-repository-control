import { Injectable, signal, effect } from '@angular/core';
import type { NotificationConfig } from './item';

const KEY = 'repositorycontrol_settings';

function load() {
  try { return JSON.parse(localStorage.getItem(KEY) ?? '{}'); } catch { return {}; }
}

@Injectable({ providedIn: 'root' })
export class StockSettingsService {
  readonly categories = signal<string[]>(
    load().categories ?? ['Alimentos', 'Higiene', 'Vestuário', 'Limpeza', 'Outros']
  );
  readonly notifications = signal<NotificationConfig>(
    load().notifications ?? { onEntry: true, onExit: true, onExpiry: true, emails: ['admin@ong.org'] }
  );

  constructor() {
    effect(() => {
      localStorage.setItem(KEY, JSON.stringify({
        categories: this.categories(),
        notifications: this.notifications(),
      }));
    });
  }

  addCategory(name: string): void {
    if (!name.trim()) return;
    this.categories.update(list => list.includes(name.trim()) ? list : [...list, name.trim()]);
  }

  removeCategory(name: string): void {
    this.categories.update(list => list.filter(c => c !== name));
  }

  updateNotifications(data: NotificationConfig): void {
    this.notifications.set(data);
  }

  addEmail(email: string): void {
    if (!email.trim()) return;
    this.notifications.update(n => n.emails.includes(email.trim()) ? n : { ...n, emails: [...n.emails, email.trim()] });
  }

  removeEmail(email: string): void {
    this.notifications.update(n => ({ ...n, emails: n.emails.filter(e => e !== email) }));
  }

  loadCategories(): void {}
  loadNotifications(): void {}
}