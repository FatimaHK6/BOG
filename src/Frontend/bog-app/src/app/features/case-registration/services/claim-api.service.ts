import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ClaimVM, ClaimsBatchUpdateDTO } from '../models/claim.model';

@Injectable({
  providedIn: 'root'
})
export class ClaimApiService {
  private baseUrl = `${environment.apiUrl}/api/case-requests`;

  constructor(private http: HttpClient) {}

  getClaims(requestId: number): Observable<ClaimVM[]> {
    return this.http.get<ClaimVM[]>(`${this.baseUrl}/${requestId}/claims`);
  }

  updateClaims(requestId: number, dto: ClaimsBatchUpdateDTO): Observable<ClaimVM[]> {
    return this.http.put<ClaimVM[]>(`${this.baseUrl}/${requestId}/claims`, dto);
  }
}
