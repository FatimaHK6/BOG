import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DefendantListVM, DefendantVM, DefendantCreateDTO } from '../models/defendant.model';

@Injectable({
  providedIn: 'root'
})
export class DefendantService {
  private readonly apiUrl = `${environment.apiUrl}/api`;

  constructor(private http: HttpClient) { }

  // Get all defendants for a case registration request
  getDefendants(requestId: number): Observable<DefendantListVM[]> {
    return this.http.get<DefendantListVM[]>(
      `${this.apiUrl}/case-requests/${requestId}/defendants`
    );
  }

  // Get defendant by ID
  getDefendant(id: number): Observable<DefendantVM> {
    return this.http.get<DefendantVM>(`${this.apiUrl}/defendants/${id}`);
  }

  // Create new defendant
  createDefendant(requestId: number, dto: DefendantCreateDTO): Observable<DefendantVM> {
    return this.http.post<DefendantVM>(
      `${this.apiUrl}/case-requests/${requestId}/defendants`,
      dto
    );
  }

  // Update defendant
  updateDefendant(id: number, dto: DefendantCreateDTO): Observable<DefendantVM> {
    return this.http.put<DefendantVM>(`${this.apiUrl}/defendants/${id}`, dto);
  }

  // Delete defendant
  deleteDefendant(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/defendants/${id}`);
  }
}
