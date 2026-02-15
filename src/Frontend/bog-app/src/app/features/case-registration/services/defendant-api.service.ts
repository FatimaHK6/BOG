import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { DefendantVM, DefendantCreateDTO, DefendantUpdateDTO } from '../models/defendant.model';

@Injectable({
  providedIn: 'root'
})
export class DefendantApiService {
  private baseUrl = `${environment.apiUrl}/api/case-registration-requests`;

  constructor(private http: HttpClient) { }

  getDefendants(requestId: number): Observable<DefendantVM[]> {
    return this.http.get<DefendantVM[]>(`${this.baseUrl}/${requestId}/defendants`);
  }

  getById(id: number): Observable<DefendantVM> {
    return this.http.get<DefendantVM>(`${this.baseUrl}/defendants/${id}`);
  }

  create(requestId: number, dto: DefendantCreateDTO): Observable<DefendantVM> {
    return this.http.post<DefendantVM>(`${this.baseUrl}/${requestId}/defendants`, dto);
  }

  update(id: number, dto: DefendantUpdateDTO): Observable<DefendantVM> {
    return this.http.put<DefendantVM>(`${this.baseUrl}/defendants/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/defendants/${id}`);
  }
}
