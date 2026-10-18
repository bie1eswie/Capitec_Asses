import { DatePipe } from '@angular/common';
import { Component, input, output } from '@angular/core';
import { AppointmentConfirmation } from '../../../shared/appointment-api';

@Component({
  selector: 'bams-booking-confirmation',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './booking-confirmation.component.html'
})
export class BookingConfirmationComponent {
  readonly appointment = input<AppointmentConfirmation | null>(null);
  readonly appointmentsRequested = output<void>();
}
