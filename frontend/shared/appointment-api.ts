import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AvailableSlot { startsAt: string; endsAt: string; }
export interface AppointmentConfirmation {
  id: string;
  reference: string;
  status: string;
  branchId: string;
  serviceId: string;
  startsAt: string;
  endsAt: string;
}
export interface BookingRequest { branchId: string; serviceId: string; startsAt: string; }

@Injectable({ providedIn: 'root' })
export class AppointmentApi {
  private readonly http = inject(HttpClient);

  availability(branchId: string, serviceId: string, date: string): Observable<AvailableSlot[]> {
    const params = new HttpParams().set('branchId', branchId).set('serviceId', serviceId).set('date', date);
    return this.http.get<AvailableSlot[]>('/api/v1/availability', { params });
  }

  book(request: BookingRequest, key: string): Observable<AppointmentConfirmation> {
    return this.http.post<AppointmentConfirmation>('/api/v1/appointments', request, {
      headers: { 'Idempotency-Key': key }
    });
  }

  mine(): Observable<AppointmentConfirmation[]> {
    return this.http.get<AppointmentConfirmation[]>('/api/v1/appointments');
  }

  cancel(id: string): Observable<AppointmentConfirmation> {
    return this.http.post<AppointmentConfirmation>(`/api/v1/appointments/${encodeURIComponent(id)}/cancel`, {});
  }

  reschedule(id: string, startsAt: string): Observable<AppointmentConfirmation> {
    return this.http.post<AppointmentConfirmation>(`/api/v1/appointments/${encodeURIComponent(id)}/reschedule`, { startsAt });
  }
}
