import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Title, Meta } from '@angular/platform-browser';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login-page',
  imports: [FormsModule],
  templateUrl: './login-page.html',
  styleUrl: './login-page.css'
})
export class LoginPage implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly title = inject(Title);
  private readonly meta = inject(Meta);

  protected isLoginMode = signal(true);
  protected name = '';
  protected email = '';
  protected password = '';
  protected error = signal(false);
  protected errorMessage = signal('');
  protected loading = signal(false);

  ngOnInit(): void {
    this.updateMetadata();
  }

  protected toggleMode(): void {
    this.isLoginMode.set(!this.isLoginMode());
    this.error.set(false);
    this.errorMessage.set('');
    this.name = '';
    this.email = '';
    this.password = '';
    this.updateMetadata();
  }

  protected doLogin(): void {
    this.loading.set(true);
    this.error.set(false);
    this.auth.login(this.email, this.password).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: err => {
        console.error('Login error:', err);
        this.error.set(true);
        this.errorMessage.set(
          err.status === 0
            ? 'Servidor indisponível. Verifique se o backend está rodando.'
            : err.error?.error ?? 'Erro ao fazer login. Tente novamente.'
        );
        this.loading.set(false);
      },
    });
  }

  protected doRegister(): void {
    this.loading.set(true);
    this.error.set(false);
    this.auth.register(this.name, this.email, this.password).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: err => {
        console.error('Register error:', err);
        this.error.set(true);

        const defaultMsg = `Erro ao criar conta: ${err.message || 'Erro desconhecido'}`

        this.errorMessage.set(
          err.status === 0
            ? 'Servidor indisponível. Verifique se o backend está rodando.'
            : err.error?.error ?? defaultMsg);
        this.loading.set(false);
      },
    });
  }

  private updateMetadata(): void {
    const isLogin = this.isLoginMode();
    const pageTitle = isLogin ? 'Login — Storager' : 'Cadastro — Storager';
    const description = isLogin
      ? 'Acesse o sistema de gerenciamento de estoque Storager.'
      : 'Crie sua conta no sistema de gerenciamento de estoque Storager.';

    this.title.setTitle(pageTitle);
    this.meta.updateTag({ name: 'description', content: description });
    this.meta.updateTag({ property: 'og:title', content: pageTitle });
    this.meta.updateTag({ property: 'og:description', content: description });
    this.meta.updateTag({ property: 'og:type', content: 'website' });
    this.meta.updateTag({ name: 'robots', content: 'noindex, nofollow' });
  }
}
