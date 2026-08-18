import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs';

export interface User {
  email: string;
  name: string;
  expiresAt: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  readonly user = signal<User | null>(null);
  readonly loading = signal(false);

  login(email: string, password: string) {
    return this.http.post<User>('/api/auth/login', { email, password }).pipe(
      tap(u => this.user.set(u))
    );
  }

  logout() {
    return this.http.post('/api/auth/logout', {}).pipe(
      tap(() => { this.user.set(null); this.router.navigate(['/login']); })
    );
  }

  isAuthenticated(): boolean {
    return this.user() !== null;
  }
}
