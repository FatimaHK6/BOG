import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AbsherVerificationResult, AbsherPersonData } from '../models/lookup.model';

@Injectable({
  providedIn: 'root'
})
export class AbsherService {
  private readonly apiUrl = `${environment.apiUrl}/api/absher`;

  constructor(private http: HttpClient) { }

  // Verify identity through Absher
  verifyIdentity(identityNumber: string, identityTypeId: number): Observable<AbsherVerificationResult> {
    return this.http.post<AbsherVerificationResult>(`${this.apiUrl}/verify`, {
      identityNumber,
      identityTypeId
    });
  }

  // Get person data from Absher
  getPersonData(identityNumber: string, identityTypeId: number): Observable<AbsherPersonData> {
    return this.http.get<AbsherPersonData>(`${this.apiUrl}/person-data`, {
      params: {
        identityNumber,
        identityTypeId: identityTypeId.toString()
      }
    });
  }

  // Get person data with addresses
  getPersonDataWithAddresses(identityNumber: string, identityTypeId: number): Observable<AbsherPersonData> {
    return this.http.get<AbsherPersonData>(`${this.apiUrl}/person-data-full`, {
      params: {
        identityNumber,
        identityTypeId: identityTypeId.toString()
      }
    });
  }
}
