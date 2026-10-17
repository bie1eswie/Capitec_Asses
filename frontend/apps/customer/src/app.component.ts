import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { BranchApi, Branch, BranchService } from '../../../shared/branch-api';
import { AppointmentApi, AvailableSlot, AppointmentConfirmation } from '../../../shared/appointment-api';
@Component({selector:'bams-root',standalone:true,imports:[FormsModule,DatePipe],template:`
<div class="shell"><aside class="sidebar"><div class="logo">branch<b>wise.</b></div><nav class="nav"><a class="active" href="#branches">⌂ &nbsp; Book a visit</a><a href="#manage">◷ &nbsp; Manage booking</a><a href="#help">ⓘ &nbsp; Help & support</a></nav><div class="sidebar-foot">BRANCH APPOINTMENTS<br/>Phase 3 Guest Management</div></aside>
<main class="main"><header class="topline"><div><div class="eyebrow">Branchwise appointments</div><h1 class="title">Your visit, your time.</h1><p class="sub">Choose a branch, service and appointment time.</p></div><span class="tag">● &nbsp; Booking pilot</span></header>
<section class="hero"><div><div class="eyebrow" style="color:#bdedce">PLAN YOUR VISIT</div><h2>Book your branch<br/>appointment.</h2><p>Receive an on-screen confirmation and simulated notification preview.</p></div></section>
<section id="branches" class="card"><h2 class="section-title">1. Choose your branch</h2><label class="field">Search branch or city<input [ngModel]="query()" (ngModelChange)="query.set($event)" (keyup.enter)="load()" placeholder="Sandton, Rosebank, Pretoria"/></label><div style="margin-top:12px"><button class="button" (click)="load()">Search</button></div>
@if(error()){<div class="status" role="alert">{{error()}}</div>}
@if(loading()){<p class="muted">Loading branches…</p>}
<div class="list">@for(branch of branches();track branch.id){<div class="branch"><div class="round">⌖</div><div><h3>{{branch.name}}</h3><p>{{branch.address}} · {{branch.city}}</p></div><button class="button right" (click)="choose(branch)">Choose</button></div>}</div>
</section>
@if(selected()){<section class="card" style="margin-top:20px"><div class="eyebrow">{{selected()?.name}}</div><h2 class="section-title">2. Select a service</h2><div class="grid">@for(service of services();track service.id){<button class="button" (click)="chooseService(service)" [attr.aria-pressed]="chosenService()?.id===service.id">{{service.name}} · {{service.durationMinutes}} min</button>}</div>
@if(chosenService()){<h2 class="section-title" style="margin-top:24px">3. Select a date and time</h2><label class="field">Date <input type="date" [ngModel]="date()" (ngModelChange)="date.set($event);loadSlots()"/></label>
@if(slotsLoading()){<p class="muted">Checking available slots…</p>}
<div class="grid" style="margin-top:12px">@for(slot of slots();track slot.startsAt){<button class="button" (click)="chosenSlot.set(slot)" [attr.aria-pressed]="chosenSlot()?.startsAt===slot.startsAt">{{slot.startsAt | date:"HH:mm":"+0200"}}</button>}</div>
@if(!slotsLoading()&&slots().length===0){<p class="muted">No slots available for this date.</p>}
@if(chosenSlot()){<h2 class="section-title" style="margin-top:24px">4. Your details</h2><div class="grid"><label class="field">First name<input [(ngModel)]="firstName"/></label><label class="field">Last name<input [(ngModel)]="lastName"/></label><label class="field">Email<input type="email" [(ngModel)]="email"/></label><label class="field">Phone (optional)<input [(ngModel)]="phone"/></label></div><div style="margin-top:20px"><button class="button" [disabled]="booking()||!firstName.trim()||!lastName.trim()||!email.includes('@')" (click)="book()">{{booking()?'Submitting…':'Confirm appointment'}}</button></div>}
}
</section>}
@if(confirmation()){<section class="card" style="margin-top:20px"><div class="eyebrow">BOOKING CONFIRMED</div><h2 class="section-title">{{confirmation()?.reference}}</h2><p class="muted">{{confirmation()?.startsAt | date:"dd MMM yyyy, HH:mm":"+0200"}} (SAST)</p><div class="status">A simulated confirmation will be processed in the background. No actual email or SMS is sent.</div>
@if(confirmation()?.managementToken){<p class="muted">Your private management token (store it securely, it is not sent by email):</p><textarea readonly rows="3" style="width:100%">{{confirmation()?.managementToken}}</textarea><p class="muted">This token is shown only once and expires after 30 days.</p>}</section>}
<section class="card" id="manage" style="margin-top:20px"><h2 class="section-title">Manage an appointment</h2><p class="muted">Enter your appointment ID and private management token.</p><div class="grid"><label class="field">Appointment ID<input [(ngModel)]="manageId" placeholder="UUID" /></label><label class="field">Management token<input [(ngModel)]="manageToken" type="password" /></label></div>
<div style="margin-top:12px"><button class="button" (click)="findBooking()" [disabled]="!manageId||!manageToken">Find appointment</button></div>
@if(manageError()){<div class="status" role="alert">{{manageError()}}</div>}
@if(managed()){<p class="muted">{{managed()?.reference}} · {{managed()?.status}} · {{managed()?.startsAt | date:'dd MMM yyyy, HH:mm':'+0200'}} SAST</p>
@if(managed()?.status==='Confirmed'){
<label class="field">New time (UTC ISO timestamp)<input [(ngModel)]="newStartsAt" placeholder="2026-11-20T08:30:00Z" /></label>
<div class="grid" style="margin-top:12px"><button class="button" (click)="rescheduleBooking()" [disabled]="!newStartsAt">Reschedule</button><button class="button" (click)="cancelBooking()">Cancel appointment</button></div>
}}
</section><p class="footer" id="help">BAMS · Branch Appointment Management System · Scheduling pilot</p>
</main></div>`})
export class AppComponent implements OnInit {
 private branchesApi=inject(BranchApi);private appointmentsApi=inject(AppointmentApi);
 query=signal('');branches=signal<Branch[]>([]);services=signal<BranchService[]>([]);selected=signal<Branch|null>(null);chosenService=signal<BranchService|null>(null);
 loading=signal(false);error=signal('');date=signal(new Intl.DateTimeFormat('en-CA',{timeZone:'Africa/Johannesburg',year:'numeric',month:'2-digit',day:'2-digit'}).format(Date.now()+86400000));slots=signal<AvailableSlot[]>([]);
 slotsLoading=signal(false);chosenSlot=signal<AvailableSlot|null>(null);booking=signal(false);confirmation=signal<AppointmentConfirmation|null>(null);
 firstName='';lastName='';email='';phone='';private pendingKey:string|null=null;
 manageId='';manageToken='';newStartsAt='';managed=signal<AppointmentConfirmation|null>(null);manageError=signal('');
 ngOnInit(){this.load();}
 load(){this.loading.set(true);this.error.set('');this.branchesApi.list(this.query()).subscribe({next:b=>{this.branches.set(b);this.loading.set(false)},error:()=>{this.error.set('Unable to reach API.');this.loading.set(false)}})}
 choose(b:Branch){this.selected.set(b);this.services.set([]);this.chosenService.set(null);this.branchesApi.services(b.id).subscribe({next:s=>this.services.set(s),error:()=>this.error.set('Unable to load services.')})}
 chooseService(s:BranchService){this.chosenService.set(s);this.loadSlots()}
 loadSlots(){this.chosenSlot.set(null);this.slots.set([]);const b=this.selected(),s=this.chosenService();if(!b||!s||!this.date())return;this.slotsLoading.set(true);this.appointmentsApi.availability(b.id,s.id,this.date()).subscribe({next:x=>{this.slots.set(x);this.slotsLoading.set(false)},error:()=>{this.error.set('Unable to load availability.');this.slotsLoading.set(false)}})}
 book(){const b=this.selected(),s=this.chosenService(),slot=this.chosenSlot();if(!b||!s||!slot)return;this.booking.set(true);this.error.set('');this.pendingKey??=crypto.randomUUID();this.appointmentsApi.book({branchId:b.id,serviceId:s.id,startsAt:slot.startsAt,firstName:this.firstName,lastName:this.lastName,email:this.email,phone:this.phone||undefined},this.pendingKey).subscribe({next:r=>{this.confirmation.set(r);this.manageId=r.id;this.manageToken=r.managementToken??'';this.managed.set(r);this.booking.set(false);this.pendingKey=null;this.loadSlots()},error:(e)=>{this.error.set(e.status===409?'Slot unavailable or booking changed. Please refresh and select again.':'Booking failed. Retry with the same details.');this.booking.set(false)}})}
 findBooking(){this.manageError.set('');this.appointmentsApi.get(this.manageId.trim(),this.manageToken.trim()).subscribe({next:a=>this.managed.set(a),error:()=>{this.managed.set(null);this.manageError.set('Appointment not found or token expired.')}})}
 cancelBooking(){if(!confirm('Cancel this appointment?'))return;this.manageError.set('');this.appointmentsApi.cancel(this.manageId.trim(),this.manageToken.trim()).subscribe({next:a=>this.managed.set(a),error:()=>this.manageError.set('Unable to cancel appointment.')})}
 rescheduleBooking(){this.manageError.set('');this.appointmentsApi.reschedule(this.manageId.trim(),this.manageToken.trim(),this.newStartsAt.trim()).subscribe({next:a=>this.managed.set(a),error:()=>this.manageError.set('Rescheduling failed. Check UTC time and slot availability.')})}

}
