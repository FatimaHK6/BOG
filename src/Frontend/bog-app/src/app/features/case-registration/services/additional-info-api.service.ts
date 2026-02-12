import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { AdditionalInfoVM, AdditionalInfoDTO } from '../models/additional-info.model';

/**
 * API Service for Additional Information Management
 * Handles HTTP communication with the backend AdditionalInfoController
 * Supports three types of additional information based on case type:
 * - Type 1: Management Decision Cancellation (إلغاء قرار إداري)
 * - Type 2: Service/Retirement Rights (حقوق خدمة/تقاعدية)
 * - Type 3: Trademark Dispute (نزاع علامة تجارية)
 */
@Injectable({
  providedIn: 'root'
})
export class AdditionalInfoApiService {
  private baseUrl = `${environment.apiUrl}/api/case-requests`;

  constructor(private http: HttpClient) { }

  /**
   * Gets existing additional information for a case registration request
   * @param requestId The case registration request ID
   * @returns Observable of AdditionalInfoVM containing all type-specific data (if any)
   */
  getAdditionalInfo(requestId: number): Observable<AdditionalInfoVM> {
    return this.http.get<AdditionalInfoVM>(
      `${this.baseUrl}/${requestId}/additional-info`
    );
  }

  /**
   * Saves or updates additional information for a case registration request
   * The backend automatically detects which types have data and saves/updates accordingly
   * @param requestId The case registration request ID
   * @param dto The additional information data with optional fields for all three types
   * @returns Observable of saved AdditionalInfoVM with populated lookup names
   */
  saveAdditionalInfo(
    requestId: number,
    dto: AdditionalInfoDTO
  ): Observable<AdditionalInfoVM> {
    return this.http.put<AdditionalInfoVM>(
      `${this.baseUrl}/${requestId}/additional-info`,
      dto
    );
  }

  /**
   * Deletes additional information for a case registration request (soft delete)
   * @param requestId The case registration request ID
   * @returns Observable<void>
   */
  deleteAdditionalInfo(requestId: number): Observable<void> {
    return this.http.delete<void>(
      `${this.baseUrl}/${requestId}/additional-info`
    );
  }
}
