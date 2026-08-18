import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login-page',
  imports: [FormsModule],
  templateUrl: './login-page.html',
  styleUrl: './login-page.css'
})
export class LoginPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  protected email = '';
  protected password = '';
  protected error = signal(false);
  protected errorMessage = signal('');
  protected loading = signal(false);

  protected doLogin(): void {
    this.loading.set(true);
    this.error.set(false);
    this.auth.login(this.email, this.password).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: err => {
        console.error('Login error:', err);
        this.error.set(true);
        this.errorMessage.set(err.status === 0 ? 'Servidor indisponível. Verifique se o backend está rodando.' : 'Email ou senha inválidos.');
        this.loading.set(false);
      },
    });
  }
}
