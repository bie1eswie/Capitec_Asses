import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
export interface AvailableSlot { startsAt: string; endsAt: string; }
export interface AppointmentConfirmation { id: string; reference: string; status: string; branchId: string; serviceId: string; startsAt: string; endsAt: string; managementToken?: string | null; }
export interface BookingRequest { branchId:string;serviceId:string;startsAt:string;firstName:string;lastName:string;email:string;phone?:string; }
@Injectable({providedIn:'root'})
export class AppointmentApi {
 private readonly http=inject(HttpClient);
 availability(branchId:string,serviceId:string,date:string):Observable<AvailableSlot[]> {
  return this.http.get<AvailableSlot[]>('/api/v1/availability', {params:new HttpParams().set('branchId',branchId).set('serviceId',serviceId).set('date',date)});
 }
 get(id:string,token:string):Observable<AppointmentConfirmation> {
  return this.http.get<AppointmentConfirmation>(`/api/v1/appointments/${id}`, {headers:{'X-Appointment-Token':token}});
 }
 cancel(id:string,token:string):Observable<AppointmentConfirmation> {
  return this.http.post<AppointmentConfirmation>(`/api/v1/appointments/${id}/cancel`,{}, {headers:{'X-Appointment-Token':token}});
 }
 reschedule(id:string,token:string,startsAt:string):Observable<AppointmentConfirmation> {
  return this.http.post<AppointmentConfirmation>(`/api/v1/appointments/${id}/reschedule`,{startsAt}, {headers:{'X-Appointment-Token':token}});
 }
 book(request:BookingRequest,key:string):Observable<AppointmentConfirmation> {
  return this.http.post<AppointmentConfirmation>('/api/v1/appointments',request,{headers:{'Idempotency-Key':key}});
 }
}
