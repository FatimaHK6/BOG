import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { RelatedCaseVM, RelatedCasesBatchUpdateDTO } from '../models/related-case.model';

@Injectable({
  providedIn: 'root'
})
export class RelatedCaseApiService {
  private baseUrl = `${environment.apiUrl}/api/case-registration-requests`;

  constructor(private http: HttpClient) {}

  getRelatedCases(requestId: number): Observable<RelatedCaseVM[]> {
    return this.http.get<RelatedCaseVM[]>(`${this.baseUrl}/${requestId}/related-cases`);
  }

  updateRelatedCases(requestId: number, dto: RelatedCasesBatchUpdateDTO): Observable<RelatedCaseVM[]> {
    return this.http.put<RelatedCaseVM[]>(`${this.baseUrl}/${requestId}/related-cases`, dto);
  }
}
