import { RouterOutlet, Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { filter } from 'rxjs';
import { Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthApi } from '../../../shared/auth-api';
import { BranchApi, Branch, BranchService } from '../../../shared/branch-api';
import { AppointmentApi, AppointmentConfirmation, AvailableSlot } from '../../../shared/appointment-api';
import { AuthPanelComponent, AuthPage } from './auth-panel.component';
import { AppointmentListComponent } from './appointment-list.component';
import { CustomerHomeComponent } from './customer-home.component';
import { BookingConfirmationComponent } from './booking-confirmation.component';
import { ConfirmDialogComponent } from './confirm-dialog.component';

type CustomerPage = 'login' | 'register' | 'forgot' | 'home' | 'branches' | 'service' | 'availability' | 'review' | 'confirmation' | 'appointments' | 'reschedule';

@Component({
  selector: 'bams-root',
  standalone: true,
  imports: [FormsModule, DatePipe, AuthPanelComponent, AppointmentListComponent, CustomerHomeComponent, BookingConfirmationComponent, ConfirmDialogComponent, RouterOutlet],
  templateUrl: './app.component.html'
})
export class AppComponent {
  readonly auth = inject(AuthApi);
  private readonly router = inject(Router);
  private readonly activeRoute = inject(ActivatedRoute);
  private readonly branchApi = inject(BranchApi);
  private readonly appointmentApi = inject(AppointmentApi);

  page = signal<CustomerPage>('login');
  error = signal('');
  message = signal('');
  busy = signal(false);
  branches = signal<Branch[]>([]);
  services = signal<BranchService[]>([]);
  slots = signal<AvailableSlot[]>([]);
  appointments = signal<AppointmentConfirmation[]>([]);
  selectedBranch = signal<Branch | null>(null);
  selectedService = signal<BranchService | null>(null);
  selectedSlot = signal<AvailableSlot | null>(null);
  confirmation = signal<AppointmentConfirmation | null>(null);
  search = '';
  firstName = '';
  lastName = '';
  email = '';
  password = '';
  phone = '';
  date = new Intl.DateTimeFormat('en-CA', {
    timeZone: 'Africa/Johannesburg', year: 'numeric', month: '2-digit', day: '2-digit'
  }).format(Date.now() + 86400000);
  private bookingKey: string | null = null;
  readonly pendingCancellation = signal<AppointmentConfirmation | null>(null);

  readonly authPage = computed<AuthPage>(() => {
    const current = this.page();
    return current === 'register' || current === 'forgot' ? current : 'login';
  });
  constructor() {
    this.router.events.pipe(filter(event => event instanceof NavigationEnd)).subscribe(() => {
      const page = this.router.url.split('?')[0].slice(1) as CustomerPage;
      this.page.set(page || 'login');
      if (page === 'branches') {
        this.loadBranches();
      }
      if (page === 'appointments') {
        this.loadAppointments();
      }
    });
  }

  readonly rescheduling = signal<AppointmentConfirmation | null>(null);
  readonly rescheduleSlots = signal<AvailableSlot[]>([]);
  readonly rescheduleSlot = signal<AvailableSlot | null>(null);
  rescheduleDate = this.date;

  loginWith(credentials: { email: string; password: string }): void {
    this.email = credentials.email;
    this.password = credentials.password;
    this.login();
  }

  registerWith(details: { firstName: string; lastName: string; email: string; password: string; phone?: string }): void {
    Object.assign(this, details);
    this.register();
  }

  forgotWith(email: string): void {
    this.email = email;
    this.forgot();
  }

  go(page: CustomerPage): void {
    this.error.set('');
    this.message.set('');
    void this.router.navigate(['/', page]);
  }

  login(): void {
    this.busy.set(true);
    this.error.set('');
    this.auth.login(this.email, this.password).subscribe({
      next: () => {
        this.busy.set(false);
        const requested = this.activeRoute.snapshot.queryParamMap.get('returnUrl');
        const target = requested?.startsWith('/') && !requested.startsWith('//') && !requested.includes('://')
          ? requested
          : '/home';
        void this.router.navigateByUrl(target);
      },
      error: () => { this.busy.set(false); this.error.set('Unable to sign in. Check your email and password.'); }
    });
  }

  register(): void {
    this.busy.set(true);
    this.error.set('');
    this.auth.register({ firstName: this.firstName, lastName: this.lastName, email: this.email,
      password: this.password, phone: this.phone || undefined }).subscribe({
      next: () => { this.busy.set(false); this.go('login'); this.message.set('Account created. You can now sign in.'); },
      error: () => { this.busy.set(false); this.error.set('Registration failed. Check the details and password requirements.'); }
    });
  }

  forgot(): void {
    this.busy.set(true);
    this.auth.forgotPassword(this.email).subscribe({
      next: () => { this.busy.set(false); this.message.set('If the account exists, password reset instructions will be sent.'); },
      error: () => { this.busy.set(false); this.error.set('Could not submit your request.'); }
    });
  }

  loadBranches(): void {
    this.busy.set(true);
    this.branchApi.list(this.search).subscribe({
      next: branches => { this.branches.set(branches); this.busy.set(false); },
      error: () => { this.error.set('Unable to load branches. Check your connection.'); this.busy.set(false); }
    });
  }

  chooseBranch(branch: Branch): void {
    this.selectedBranch.set(branch);
    this.selectedService.set(null);
    this.selectedSlot.set(null);
    this.busy.set(true);
    this.branchApi.services(branch.id).subscribe({
      next: services => { this.services.set(services); this.busy.set(false); this.go('service'); },
      error: () => { this.busy.set(false); this.error.set('Unable to load branch services.'); }
    });
  }

  chooseService(service: BranchService): void {
    this.selectedService.set(service);
    this.loadSlots();
    this.go('availability');
  }

  loadSlots(): void {
    const branch = this.selectedBranch();
    const service = this.selectedService();
    if (!branch || !service || !this.date) { return; }
    this.selectedSlot.set(null);
    this.slots.set([]);
    this.busy.set(true);
    this.appointmentApi.availability(branch.id, service.id, this.date).subscribe({
      next: slots => { this.slots.set(slots); this.busy.set(false); },
      error: () => { this.error.set('Unable to load available times.'); this.busy.set(false); }
    });
  }

  confirmBooking(): void {
    const branch = this.selectedBranch();
    const service = this.selectedService();
    const slot = this.selectedSlot();
    if (!branch || !service || !slot) { return; }
    this.busy.set(true);
    this.error.set('');
    this.bookingKey ??= crypto.randomUUID();
    this.appointmentApi.book({ branchId: branch.id, serviceId: service.id,
      startsAt: slot.startsAt }, this.bookingKey).subscribe({
      next: confirmation => { this.confirmation.set(confirmation); this.bookingKey = null;
        this.busy.set(false); this.go('confirmation'); },
      error: response => { this.busy.set(false);
        this.error.set(response.status === 409 ? 'This slot is no longer available. Please select another.' : 'Booking failed. Please retry.'); }
    });
  }

  loadAppointments(): void {
    this.busy.set(true);
    this.appointmentApi.mine().subscribe({
      next: appointments => { this.appointments.set(appointments); this.busy.set(false); },
      error: () => { this.error.set('Unable to load your appointments.'); this.busy.set(false); }
    });
  }

  cancel(appointment: AppointmentConfirmation): void {
    this.pendingCancellation.set(appointment);
  }

  confirmCancellation(): void {
    const appointment = this.pendingCancellation();
    if (!appointment) return;

    this.pendingCancellation.set(null);
    this.busy.set(true);
    this.appointmentApi.cancel(appointment.id).subscribe({
      next: () => this.loadAppointments(),
      error: () => { this.busy.set(false); this.error.set('Unable to cancel this appointment.'); }
    });
  }

  dismissCancellation(): void {
    this.pendingCancellation.set(null);
  }

  reschedule(appointment: AppointmentConfirmation): void {
    this.rescheduling.set(appointment);
    this.rescheduleSlot.set(null);
    this.rescheduleSlots.set([]);
    this.rescheduleDate = this.date;
    this.go('reschedule');
    this.loadRescheduleSlots();
  }

  loadRescheduleSlots(): void {
    const appointment = this.rescheduling();
    if (!appointment || !this.rescheduleDate) {
      return;
    }

    this.busy.set(true);
    this.error.set('');
    this.rescheduleSlot.set(null);
    this.rescheduleSlots.set([]);
    this.appointmentApi.availability(
      appointment.branchId,
      appointment.serviceId,
      this.rescheduleDate
    ).subscribe({
      next: slots => {
        this.rescheduleSlots.set(slots);
        this.busy.set(false);
      },
      error: () => {
        this.error.set('Could not load available times. Please retry.');
        this.busy.set(false);
      }
    });
  }

  saveReschedule(): void {
    const appointment = this.rescheduling();
    const slot = this.rescheduleSlot();

    if (!appointment || !slot) {
      return;
    }

    this.busy.set(true);
    this.error.set('');
    this.appointmentApi.reschedule(appointment.id, slot.startsAt).subscribe({
      next: () => {
        this.rescheduling.set(null);
        this.go('appointments');
        this.message.set('Appointment rescheduled successfully.');
      },
      error: response => {
        this.busy.set(false);
        this.error.set(response.status === 409
          ? 'This time is no longer available. Please select another.'
          : 'Unable to reschedule. Please retry.');
      }
    });
  }

  async logout(): Promise<void> {
    const revoked = await this.auth.logout();
    this.appointments.set([]);
    this.branches.set([]);
    this.go('login');
    if (!revoked) {
      this.error.set('Signed out locally. The server could not confirm session revocation.');
    }
  }
}
