import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { DeficiencyVM, DeficienciesBatchUpdateDTO } from '../models/deficiency.model';

@Injectable({ providedIn: 'root' })
export class DeficienciesApiService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /**
   * GET /api/case-requests/{requestId}/deficiencies
   * Load all deficiencies for a case request
   */
  getDeficiencies(requestId: number): Observable<DeficiencyVM[]> {
    return this.http.get<DeficiencyVM[]>(
      `${this.baseUrl}/api/case-requests/${requestId}/deficiencies`
    );
  }

  /**
   * PUT /api/case-requests/{requestId}/deficiencies
   * Update all deficiencies for a case request (batch update)
   */
  updateDeficiencies(requestId: number, dto: DeficienciesBatchUpdateDTO): Observable<DeficiencyVM[]> {
    return this.http.put<DeficiencyVM[]>(
      `${this.baseUrl}/api/case-requests/${requestId}/deficiencies`,
      dto
    );
  }
}
