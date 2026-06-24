import { Routes } from '@angular/router';
import { autoLoginPartialRoutesGuard } from 'angular-auth-oidc-client';
import { AppLayout } from './shared/layouts/app-layout/app-layout';

export const routes: Routes = [
  {
    path: '',
    component: AppLayout,
    canActivateChild: [autoLoginPartialRoutesGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'lobby' },
      { path: 'lobby', loadComponent: () => import('./lobby/lobby').then(m => m.LobbyComponent) },
      { path: 'leaderboard', loadComponent: () => import('./leaderboard/leaderboard').then(m => m.LeaderboardComponent) },
      { path: 'history', loadComponent: () => import('./history/history').then(m => m.HistoryComponent) },
    ],
  },
  { path: '**', redirectTo: '' },
];
