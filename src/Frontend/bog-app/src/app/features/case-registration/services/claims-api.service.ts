import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { ClaimVM, ClaimDTO } from '../models/claim.model';

@Injectable({ providedIn: 'root' })
export class ClaimsApiService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /**
   * GET /api/case-requests/{requestId}/claims
   * Load all claims for a case request
   */
  getClaims(requestId: number): Observable<ClaimVM[]> {
    return this.http.get<ClaimVM[]>(
      `${this.baseUrl}/api/case-requests/${requestId}/claims`
    );
  }

  /**
   * PUT /api/case-requests/{requestId}/claims
   * Update all claims for a case request
   */
  updateClaims(requestId: number, claims: ClaimDTO[]): Observable<ClaimVM[]> {
    const payload = { claims };
    return this.http.put<ClaimVM[]>(
      `${this.baseUrl}/api/case-requests/${requestId}/claims`,
      payload
    );
  }
}
