import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CaseRegistrationRequestListVM {
  id: number;
  requestStatusId: number;
  statusNameAr: string;
  subjectPreview: string;
  courtNameAr: string;
  caseNumber: string;
  plaintiffsCount: number;
  defendantsCount: number;
  createdDate: Date;
  modifiedDate: Date;
  isDraft: boolean;
  caseTypeId: number;
  plaintiffName: string;
  defendantName: string;
}

export interface CaseRegistrationRequestVM {
  id: number;
  requestStatusId: number;
  statusNameAr: string;
  statusName: string;
  subject: string;
  evidence: string;
  notes: string;
  courtId: number;
  courtNameAr: string;
  submissionDate: Date;
  completionDeadline: Date;
  rejectionReason: string;
  caseNumber: string;
  registrationNumber: string;
  registrationDate: Date;
  createdDate: Date;
  modifiedDate: Date;
  plaintiffsCount: number;
  defendantsCount: number;
  createdByUserName: string;
}

export interface CaseRegistrationRequestCreateDTO {
  subject?: string;
  evidence?: string;
  notes?: string;
  courtId?: number;
  saveAsDraft: boolean;
  plaintiffIds?: number[];
  defendantIds?: number[];
}

export interface CaseRegistrationRequestUpdateDTO {
  subject?: string;
  evidence?: string;
  notes?: string;
  courtId?: number;
  saveAsDraft: boolean;
  plaintiffIds?: number[];
  defendantIds?: number[];
}

@Injectable({
  providedIn: 'root'
})
export class CaseRegistrationRequestService {
  private readonly apiUrl = `${environment.apiUrl}/api/case-registration-requests`;

  constructor(private http: HttpClient) { }

  // Get all requests for the current user
  getAll(): Observable<CaseRegistrationRequestListVM[]> {
    return this.http.get<CaseRegistrationRequestListVM[]>(this.apiUrl);
  }

  // Get all draft requests
  getDrafts(): Observable<CaseRegistrationRequestListVM[]> {
    return this.http.get<CaseRegistrationRequestListVM[]>(`${this.apiUrl}/drafts`);
  }

  // Get request by ID
  getById(id: number): Observable<CaseRegistrationRequestVM> {
    return this.http.get<CaseRegistrationRequestVM>(`${this.apiUrl}/${id}`);
  }

  // Get or create draft
  getOrCreateDraft(): Observable<CaseRegistrationRequestVM> {
    return this.http.get<CaseRegistrationRequestVM>(`${this.apiUrl}/current-draft`);
  }

  // Create new draft (always creates a new one)
  createNewDraft(): Observable<CaseRegistrationRequestVM> {
    return this.http.post<CaseRegistrationRequestVM>(this.apiUrl, { saveAsDraft: true });
  }

  // Create new request
  create(dto: CaseRegistrationRequestCreateDTO): Observable<CaseRegistrationRequestVM> {
    return this.http.post<CaseRegistrationRequestVM>(this.apiUrl, dto);
  }

  // Update request
  update(id: number, dto: CaseRegistrationRequestUpdateDTO): Observable<CaseRegistrationRequestVM> {
    return this.http.put<CaseRegistrationRequestVM>(`${this.apiUrl}/${id}`, dto);
  }

  // Delete request
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // Submit request
  submit(id: number): Observable<CaseRegistrationRequestVM> {
    return this.http.post<CaseRegistrationRequestVM>(`${this.apiUrl}/${id}/submit`, {});
  }
}
