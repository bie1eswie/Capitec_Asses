import { DatePipe } from '@angular/common';
import { Component, input, output } from '@angular/core';
import { AppointmentConfirmation } from '../../../shared/appointment-api';

@Component({
  selector: 'bams-appointment-list',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './appointment-list.component.html'
})
export class AppointmentListComponent {
  readonly appointments = input.required<AppointmentConfirmation[]>();
  readonly busy = input(false);
  readonly cancelRequested = output<AppointmentConfirmation>();
  readonly rescheduleRequested = output<AppointmentConfirmation>();
  readonly newRequested = output<void>();
}
