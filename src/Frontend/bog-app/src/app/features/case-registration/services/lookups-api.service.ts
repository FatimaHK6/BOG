import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { ClassificationVM } from '../models/case-request.model';
import { NotificationMethodVM, GovernmentEntityVM } from '../models/additional-info.model';
import { CourtLookup } from '../models/related-case.model';

/**
 * API Service for Lookup Data Management
 * Handles HTTP communication with the backend LookupsController
 * Provides lookups for various dropdowns and selection fields
 */
@Injectable({
  providedIn: 'root'
})
export class LookupsApiService {
  private baseUrl = `${environment.apiUrl}/api/lookups`;

  constructor(private http: HttpClient) { }

  /**
   * Gets all active classifications
   * Used for the case classification dropdown
   * @returns Observable array of ClassificationVM
   */
  getClassifications(): Observable<ClassificationVM[]> {
    return this.http.get<ClassificationVM[]>(`${this.baseUrl}/classifications`);
  }

  /**
   * Gets all active attachment types
   * Used for the attachment type dropdown when uploading attachments
   * @returns Observable array of attachment type objects
   */
  getAttachmentTypes(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/attachment-types`);
  }

  /**
   * Gets all active notification methods
   * Used in Additional Info Type 1 (Management Decision) for the notification method dropdown
   * طريقة العلم بالقرار
   * @returns Observable array of NotificationMethodVM
   */
  getNotificationMethods(): Observable<NotificationMethodVM[]> {
    return this.http.get<NotificationMethodVM[]>(
      `${this.baseUrl}/notification-methods`
    );
  }

  /**
   * Gets all active government entities
   * Used in Additional Info for:
   * - Type 1: Decision Issuing Authority dropdown (جهة إصدار القرار)
   * - Type 2: Authority Complained To dropdown (الجهة المتظلم لها)
   * @returns Observable array of GovernmentEntityVM
   */
  getGovernmentEntities(): Observable<GovernmentEntityVM[]> {
    return this.http.get<GovernmentEntityVM[]>(
      `${this.baseUrl}/government-entities`
    );
  }

  /**
   * Gets all active courts
   * Used in the Related Cases section for the court dropdown
   * @returns Observable array of CourtLookup
   */
  getCourts(): Observable<CourtLookup[]> {
    return this.http.get<CourtLookup[]>(`${this.baseUrl}/courts`);
  }
}
