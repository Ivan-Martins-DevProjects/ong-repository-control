import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { StockSettingsService } from '../stock/stock-settings.service';

@Component({
  selector: 'app-notifications-page',
  imports: [FormsModule],
  templateUrl: './notifications-page.html',
  styleUrl: './notifications-page.css'
})
export class NotificationsPage {
  protected readonly settings = inject(StockSettingsService);
  protected config = { ...this.settings.notifications() };
  protected newEmail = '';

  constructor() {
    this.config = { ...this.settings.notifications() };
  }

  protected addEmail(): void {
    if (this.newEmail.trim()) {
      this.settings.addEmail(this.newEmail);
      this.config = { ...this.settings.notifications() };
      this.newEmail = '';
    }
  }

  protected removeEmail(email: string): void {
    this.settings.removeEmail(email);
    this.config = { ...this.settings.notifications() };
  }

  protected toggle(key: 'onEntry' | 'onExit' | 'onExpiry'): void {
    this.config[key] = !this.config[key];
    this.settings.updateNotifications({ ...this.config });
  }
}