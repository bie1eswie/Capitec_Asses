import { Routes } from '@angular/router';
import { authGuard } from '../../../shared/auth/guards/auth.guard';
import { roleGuard } from '../../../shared/auth/guards/role.guard';
import { permissionGuard } from '../../../shared/auth/guards/permission.guard';
import { RouteViewComponent } from './route-view.component';

const staffRoles = ['SystemAdministrator', 'BranchManager', 'BranchStaff'];

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: 'login', component: RouteViewComponent },
  { path: 'unauthorized', component: RouteViewComponent },
  ...['dashboard', 'branches'].map(path => ({
    path, component: RouteViewComponent, canActivate: [authGuard, roleGuard], data: { roles: staffRoles }
  })),
  // Privileged pages remain closed until the API issues verified permission claims.
  { path: 'appointments', component: RouteViewComponent, canActivate: [authGuard, roleGuard, permissionGuard], data: { roles: staffRoles, permissions: ['appointments.read.branch'] } },
  { path: 'services', component: RouteViewComponent, canActivate: [authGuard, roleGuard, permissionGuard], data: { roles: staffRoles, permissions: ['services.manage'] } },
  { path: 'schedules', component: RouteViewComponent, canActivate: [authGuard, roleGuard, permissionGuard], data: { roles: staffRoles, permissions: ['branches.schedules.manage'] } },
  { path: 'staff', component: RouteViewComponent, canActivate: [authGuard, roleGuard, permissionGuard], data: { roles: ['SystemAdministrator', 'BranchManager'], permissions: ['staff.manage'] } },
  { path: 'notifications', component: RouteViewComponent, canActivate: [authGuard, roleGuard, permissionGuard], data: { roles: ['SystemAdministrator'], permissions: ['notifications.read'] } },
  { path: 'reports', component: RouteViewComponent, canActivate: [authGuard, roleGuard, permissionGuard], data: { roles: ['SystemAdministrator', 'BranchManager'], permissions: ['reports.read'] } },
  { path: '**', redirectTo: 'dashboard' }
];
