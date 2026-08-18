import { Routes } from '@angular/router';
import { LoginPage } from './modules/login/login-page';
import { DashboardPage } from './modules/dashboard/dashboard-page';
import { StockPage } from './modules/stock/stock-page';
import { StockItemsPage } from './modules/stock/stock-items-page';
import { HistoryPage } from './modules/history/history-page';
import { CategoriesPage } from './modules/settings/categories-page';
import { NotificationsPage } from './modules/settings/notifications-page';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginPage },
  { path: 'dashboard', component: DashboardPage },
  { path: 'estoque', component: StockPage },
  { path: 'estoque/:name', component: StockItemsPage },
  { path: 'historico', component: HistoryPage },
  { path: 'configuracoes', redirectTo: 'configuracoes/categorias', pathMatch: 'full' },
  { path: 'configuracoes/categorias', component: CategoriesPage },
  { path: 'configuracoes/notificacoes', component: NotificationsPage },
];