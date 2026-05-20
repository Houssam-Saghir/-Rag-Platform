import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { ShellComponent } from './layout/shell.component';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./features/auth/register.component').then(m => m.RegisterComponent) },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent) },
      { path: 'documents', loadComponent: () => import('./features/documents/documents.component').then(m => m.DocumentsComponent) },
      { path: 'chat', loadComponent: () => import('./features/chat/chat.component').then(m => m.ChatComponent) },
      { path: 'admin', loadComponent: () => import('./features/admin/admin.component').then(m => m.AdminComponent), canActivate: [roleGuard], data: { roles: ['Admin'] } }
    ]
  },
  { path: '**', redirectTo: '' }
];
