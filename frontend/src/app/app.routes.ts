import { Routes } from '@angular/router';
import { LoginPage } from './modules/login/login-page';
import { DashboardPage } from './modules/dashboard/dashboard-page';
import { Stock } from './modules/stock/stock';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginPage },
  { path: 'dashboard', component: DashboardPage },
  { path: 'estoque', component: Stock },
  { path: 'configuracoes', redirectTo: 'configuracoes/categorias', pathMatch: 'full' },
];
