import { Component, inject, signal } from '@angular/core';
import { RouterOutlet, Router } from '@angular/router';
import { Sidebar } from './components/sidebar/sidebar';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Sidebar],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  protected readonly hideSidebar = toSignal(
    this.router.events.pipe(
      map(() => this.router.url === '/login')
    ),
    { initialValue: this.router.url === '/login' }
  );
  protected readonly sidebarOpen = signal(false);

  constructor() {}

  protected toggleSidebar(): void {
    this.sidebarOpen.update(v => !v);
  }

  protected closeSidebar(): void {
    this.sidebarOpen.set(false);
  }
}