import { Routes } from '@angular/router';
import { authGuard } from './guards/auth-guard';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', loadComponent: () => import('./components/login/login.component').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./components/register/register.component').then(m => m.RegisterComponent) },
  { path: 'dashboard', loadComponent: () => import('./components/dashboard/dashboard.component').then(m => m.DashboardComponent), canActivate: [authGuard] },
  { path: 'create-room', loadComponent: () => import('./components/create-room/create-room.component').then(m => m.CreateRoomComponent), canActivate: [authGuard] },
  { path: 'join-room', loadComponent: () => import('./components/join-room/join-room.component').then(m => m.JoinRoomComponent), canActivate: [authGuard] },
  { path: 'game/:roomId', loadComponent: () => import('./components/game-room/game-room.component').then(m => m.GameRoomComponent) },
  { path: '**', redirectTo: '/login' }
];
