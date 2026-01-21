import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { RepresentativeVM } from '../models/plaintiff.model';
import {
  RepresentativeCreateDTO,
  RepresentativeUpdateDTO,
  RepresentativeType
} from '../models/representative.model';

@Injectable({
  providedIn: 'root'
})
export class RepresentativeService {
  private readonly apiUrl = `${environment.apiUrl}/api`;

  constructor(private http: HttpClient) { }

  // Get all representatives for a plaintiff
  getRepresentatives(plaintiffId: number): Observable<RepresentativeVM[]> {
    return this.http.get<RepresentativeVM[]>(
      `${this.apiUrl}/plaintiffs/${plaintiffId}/representatives`
    );
  }

  // Get representative by ID
  getRepresentative(id: number): Observable<RepresentativeVM> {
    return this.http.get<RepresentativeVM>(`${this.apiUrl}/representatives/${id}`);
  }

  // Create new representative
  createRepresentative(plaintiffId: number, dto: RepresentativeCreateDTO): Observable<RepresentativeVM> {
    return this.http.post<RepresentativeVM>(
      `${this.apiUrl}/plaintiffs/${plaintiffId}/representatives`,
      dto
    );
  }

  // Update representative
  updateRepresentative(id: number, dto: RepresentativeUpdateDTO): Observable<RepresentativeVM> {
    return this.http.put<RepresentativeVM>(`${this.apiUrl}/representatives/${id}`, dto);
  }

  // Delete representative
  deleteRepresentative(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/representatives/${id}`);
  }

  // Set representative as applicant
  setAsApplicant(id: number): Observable<RepresentativeVM> {
    return this.http.post<RepresentativeVM>(
      `${this.apiUrl}/representatives/${id}/set-applicant`,
      {}
    );
  }

  // Get representative types
  getRepresentativeTypes(): Observable<RepresentativeType[]> {
    return this.http.get<RepresentativeType[]>(`${this.apiUrl}/lookups/representative-types`);
  }
}
