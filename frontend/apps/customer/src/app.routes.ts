import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/auth/guards/auth.guard';
import { guestGuard } from '../../../shared/auth/guards/guest.guard';
import { RouteViewComponent } from './route-view.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'home' },
  ...['login', 'register', 'forgot'].map(path => ({ path, component: RouteViewComponent, canActivate: [guestGuard] })),
  ...['home', 'branches', 'service', 'availability', 'review', 'confirmation', 'appointments', 'reschedule'].map(path => ({
    path, component: RouteViewComponent, canActivate: [authGuard]
  })),
  { path: '**', redirectTo: 'home' }
];
