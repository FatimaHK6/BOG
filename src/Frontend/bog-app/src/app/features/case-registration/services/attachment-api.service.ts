import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

export interface RequestAttachmentVM {
  id: number;
  requestId: number;
  attachmentTypeId: number;
  attachmentTypeName: string;
  fileName: string;
  fileSize: number;
  fileSizeKb: number;
  isMandatory: boolean;
  createdDate: Date;
  modifiedDate: Date;
  downloadUrl?: string;
  description?: string;
}

export interface RequestAttachmentCreateDTO {
  attachmentTypeId: number;
  fileName: string;
  fileContent: string; // Base64 encoded
  contentType: string;
  description?: string;
}

export interface AttachmentTypeVM {
  id: number;
  name: string;
  nameAr: string;
  isMandatory: boolean;
  description?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AttachmentApiService {
  private apiUrl = `${environment.apiUrl}/api/case-registration-requests`;
  private lookupsUrl = `${environment.apiUrl}/api/lookups`;

  constructor(private http: HttpClient) { }

  uploadAttachment(requestId: number, dto: RequestAttachmentCreateDTO): Observable<RequestAttachmentVM> {
    return this.http.post<RequestAttachmentVM>(`${this.apiUrl}/${requestId}/attachments`, dto);
  }

  getAttachments(requestId: number): Observable<RequestAttachmentVM[]> {
    return this.http.get<RequestAttachmentVM[]>(`${this.apiUrl}/${requestId}/attachments`);
  }

  downloadAttachment(requestId: number, attachmentId: number): Observable<Blob> {
    return this.http.get(
      `${this.apiUrl}/${requestId}/attachments/${attachmentId}/download`,
      { responseType: 'blob' }
    );
  }

  deleteAttachment(requestId: number, attachmentId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${requestId}/attachments/${attachmentId}`);
  }

  getAttachmentTypes(): Observable<AttachmentTypeVM[]> {
    return this.http.get<AttachmentTypeVM[]>(`${this.lookupsUrl}/attachment-types`);
  }
}
