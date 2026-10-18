import { RouterOutlet, Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { filter } from 'rxjs';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthApi } from '../../../shared/auth-api';
import { BranchApi, Branch } from '../../../shared/branch-api';
import { FeatureUnavailableComponent } from './feature-unavailable.component';

type AdminPage = 'dashboard' | 'appointments' | 'branches' | 'services' | 'schedules' | 'staff' | 'notifications' | 'reports';

@Component({
  selector: 'bams-root',
  standalone: true,
  imports: [FormsModule, FeatureUnavailableComponent, RouterOutlet],
  templateUrl: './app.component.html'
})
export class AppComponent {
  readonly auth = inject(AuthApi);
  private readonly router = inject(Router);
  private readonly activeRoute = inject(ActivatedRoute);
  private readonly branchApi = inject(BranchApi);
  page = signal<AdminPage>('dashboard');
  branches = signal<Branch[]>([]);
  error = signal('');
  busy = signal(false);
  email = '';
  password = '';
  search = '';
  readonly menu: { id: AdminPage; label: string; icon: string }[] = [
    { id: 'dashboard', label: 'Dashboard', icon: '▦' },
    { id: 'appointments', label: 'Appointments', icon: '▤' },
    { id: 'branches', label: 'Branches', icon: '⌖' },
    { id: 'services', label: 'Services', icon: '◫' },
    { id: 'schedules', label: 'Schedules', icon: '▦' },
    { id: 'staff', label: 'Staff', icon: '♙' },
    { id: 'notifications', label: 'Notifications', icon: '✉' },
    { id: 'reports', label: 'Reports', icon: '▥' }
  ];

  constructor() {
    this.router.events.pipe(filter(event => event instanceof NavigationEnd)).subscribe(() => {
      const page = this.router.url.split('?')[0].slice(1) as AdminPage;
      if (this.menu.some(item => item.id === page)) {
        this.page.set(page);
        if (page === 'branches' || page === 'dashboard') {
          this.loadBranches();
        }
      }
    });
  }

  canNavigate(page: AdminPage): boolean {
    const requiredPermissions: Partial<Record<AdminPage, string>> = {
      appointments: 'appointments.read.branch',
      services: 'services.manage',
      schedules: 'branches.schedules.manage',
      staff: 'staff.manage',
      notifications: 'notifications.read',
      reports: 'reports.read'
    };

    const required = requiredPermissions[page];
    const roles = this.auth.session()?.roles ?? [];
    if (page === 'notifications' && !roles.includes('SystemAdministrator')) {
      return false;
    }
    if ((page === 'staff' || page === 'reports') &&
        !roles.some(role => role === 'SystemAdministrator' || role === 'BranchManager')) {
      return false;
    }
    return !required || (this.auth.session()?.permissions ?? []).includes(required);
  }

  get allowed(): boolean {
    const roles = this.auth.session()?.roles ?? [];
    return roles.some(role => ['SystemAdministrator', 'BranchManager', 'BranchStaff'].includes(role));
  }

  login(): void {
    this.busy.set(true);
    this.error.set('');
    this.auth.login(this.email, this.password).subscribe({
      next: () => {
        this.busy.set(false);
        if (this.allowed) {
          const requested = this.activeRoute.snapshot.queryParamMap.get('returnUrl');
          const target = requested?.startsWith('/') && !requested.startsWith('//') && !requested.includes('://')
            ? requested : '/dashboard';
          void this.router.navigateByUrl(target);
        } else {
          this.error.set('This account does not have staff access.');
          void this.auth.logout();
        }
      },
      error: () => { this.busy.set(false); this.error.set('Sign-in failed. Check your credentials.'); }
    });
  }

  select(page: AdminPage): void {
    this.error.set('');
    void this.router.navigate(['/', page]);
  }

  loadBranches(): void {
    this.busy.set(true);
    this.branchApi.list(this.search).subscribe({
      next: branches => { this.branches.set(branches); this.busy.set(false); },
      error: () => { this.error.set('Unable to load branches.'); this.busy.set(false); }
    });
  }

  async logout(): Promise<void> {
    const revoked = await this.auth.logout();
    this.branches.set([]);
    void this.router.navigate(['/login']);
    if (!revoked) {
      this.error.set('Signed out locally; server revocation could not be confirmed.');
    }
  }
}
