import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
export interface Branch { id:string; code:string; name:string; city:string; address:string; timeZone:string; }
export interface BranchService { id:string; code:string; name:string; durationMinutes:number; }
@Injectable({providedIn:'root'})
export class BranchApi {
  private readonly http=inject(HttpClient);
  list(search=''):Observable<Branch[]> { return this.http.get<Branch[]>('/api/v1/branches',{params:new HttpParams().set('search',search)}); }
  services(branchId:string):Observable<BranchService[]> { return this.http.get<BranchService[]>(`/api/v1/branches/${encodeURIComponent(branchId)}/services`); }
}
