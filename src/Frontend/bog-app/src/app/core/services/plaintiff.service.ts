import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  PlaintiffListVM,
  PlaintiffVM,
  PlaintiffCreateDTO,
  PlaintiffUpdateDTO,
  SelectedAddressDTO,
  AddressVM
} from '../models/plaintiff.model';

@Injectable({
  providedIn: 'root'
})
export class PlaintiffService {
  private readonly apiUrl = `${environment.apiUrl}/api`;

  constructor(private http: HttpClient) { }

  // Get all plaintiffs for a case registration request
  getPlaintiffs(requestId: number): Observable<PlaintiffListVM[]> {
    return this.http.get<PlaintiffListVM[]>(
      `${this.apiUrl}/case-requests/${requestId}/plaintiffs`
    );
  }

  // Get plaintiff by ID
  getPlaintiff(id: number): Observable<PlaintiffVM> {
    return this.http.get<PlaintiffVM>(`${this.apiUrl}/plaintiffs/${id}`);
  }

  // Create new plaintiff
  createPlaintiff(requestId: number, dto: PlaintiffCreateDTO): Observable<PlaintiffVM> {
    return this.http.post<PlaintiffVM>(
      `${this.apiUrl}/case-requests/${requestId}/plaintiffs`,
      dto
    );
  }

  // Update plaintiff
  updatePlaintiff(id: number, dto: PlaintiffUpdateDTO): Observable<PlaintiffVM> {
    return this.http.put<PlaintiffVM>(`${this.apiUrl}/plaintiffs/${id}`, dto);
  }

  // Delete plaintiff
  deletePlaintiff(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/plaintiffs/${id}`);
  }

  // Set plaintiff as applicant
  setAsApplicant(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/plaintiffs/${id}/set-applicant`, {});
  }

  // Get selected address
  getSelectedAddress(plaintiffId: number): Observable<AddressVM> {
    return this.http.get<AddressVM>(
      `${this.apiUrl}/plaintiffs/${plaintiffId}/selected-address`
    );
  }

  // Set selected address
  setSelectedAddress(plaintiffId: number, dto: SelectedAddressDTO): Observable<AddressVM> {
    return this.http.put<AddressVM>(
      `${this.apiUrl}/plaintiffs/${plaintiffId}/selected-address`,
      dto
    );
  }
}
