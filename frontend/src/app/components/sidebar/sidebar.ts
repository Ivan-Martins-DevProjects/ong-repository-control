import { Component, inject, input, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css'
})
export class Sidebar {
  private readonly auth = inject(AuthService);
  readonly open = input(false);
  readonly navigate = output();
  protected readonly user = this.auth.user;

  protected doLogout(): void {
    this.auth.logout().subscribe();
  }
}