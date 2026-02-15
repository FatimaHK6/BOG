import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { CaseRequestVM, CaseRequestCreateDTO, CaseRequestUpdateDTO, PagedResult, RequestDecisionDTO, CaseTypeVM } from '../models/case-request.model';

@Injectable({
  providedIn: 'root'
})
export class CaseRegistrationApiService {
  private baseUrl = `${environment.apiUrl}/api/case-registration-requests`;

  constructor(private http: HttpClient) { }

  getRequests(params?: any): Observable<PagedResult<CaseRequestVM>> {
    return this.http.get<PagedResult<CaseRequestVM>>(this.baseUrl, { params });
  }

  getById(id: number): Observable<CaseRequestVM> {
    return this.http.get<CaseRequestVM>(`${this.baseUrl}/${id}`);
  }

  create(dto: CaseRequestCreateDTO): Observable<CaseRequestVM> {
    return this.http.post<CaseRequestVM>(this.baseUrl, dto);
  }

  update(id: number, dto: CaseRequestUpdateDTO): Observable<CaseRequestVM> {
    return this.http.put<CaseRequestVM>(`${this.baseUrl}/${id}`, dto);
  }

  submit(id: number): Observable<CaseRequestVM> {
    return this.http.post<CaseRequestVM>(`${this.baseUrl}/${id}/submit`, {});
  }

  register(id: number): Observable<CaseRequestVM> {
    return this.http.post<CaseRequestVM>(`${this.baseUrl}/${id}/register`, {});
  }

  reject(id: number, notes: string): Observable<CaseRequestVM> {
    return this.http.post<CaseRequestVM>(`${this.baseUrl}/${id}/reject`, { notes });
  }

  requestCompletion(id: number, deficiencies: string): Observable<CaseRequestVM> {
    return this.http.post<CaseRequestVM>(`${this.baseUrl}/${id}/request-completion`, { deficiencies });
  }

  completeRequest(id: number, decision: RequestDecisionDTO): Observable<CaseRequestVM> {
    return this.http.post<CaseRequestVM>(`${this.baseUrl}/${id}/complete`, decision);
  }

  getCaseTypes(): Observable<CaseTypeVM[]> {
    return this.http.get<CaseTypeVM[]>(`${environment.apiUrl}/api/lookups/case-types`);
  }
}
