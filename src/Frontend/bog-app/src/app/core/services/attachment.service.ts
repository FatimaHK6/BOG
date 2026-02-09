import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent, HttpEventType, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PlaintiffAttachmentVM } from '../models/plaintiff.model';
import { AttachmentType, ATTACHMENT_CONSTRAINTS } from '../models/attachment.model';

@Injectable({
  providedIn: 'root'
})
export class AttachmentService {
  private readonly apiUrl = `${environment.apiUrl}/api`;

  constructor(private http: HttpClient) { }

  // Get all attachments for a plaintiff
  getAttachments(plaintiffId: number): Observable<PlaintiffAttachmentVM[]> {
    return this.http.get<PlaintiffAttachmentVM[]>(
      `${this.apiUrl}/plaintiffs/${plaintiffId}/attachments`
    );
  }

  // Upload attachment with progress tracking
  uploadAttachment(plaintiffId: number, attachmentTypeId: number, file: File, description?: string): Observable<HttpEvent<PlaintiffAttachmentVM>> {
    return new Observable(observer => {
      // Convert file to Base64
      const reader = new FileReader();
      reader.onload = () => {
        const base64Content = (reader.result as string).split(',')[1]; // Remove data:application/pdf;base64, prefix

        const dto = {
          attachmentTypeId: attachmentTypeId,
          fileName: file.name,
          fileContent: base64Content,
          contentType: file.type,
          description: description // User note/comment
        };

        // Send progress event
        observer.next({
          type: HttpEventType.UploadProgress,
          loaded: 50,
          total: 100
        } as HttpEvent<PlaintiffAttachmentVM>);

        this.http.post<PlaintiffAttachmentVM>(
          `${this.apiUrl}/plaintiffs/${plaintiffId}/attachments`,
          dto
        ).subscribe({
          next: (result) => {
            observer.next({
              type: HttpEventType.Response,
              body: result
            } as HttpEvent<PlaintiffAttachmentVM>);
            observer.complete();
          },
          error: (err) => observer.error(err)
        });
      };
      reader.onerror = (error) => observer.error(error);
      reader.readAsDataURL(file);
    });
  }

  // Delete attachment
  deleteAttachment(plaintiffId: number, attachmentId: number): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/plaintiffs/${plaintiffId}/attachments/${attachmentId}`
    );
  }

  // Get attachment types
  getAttachmentTypes(): Observable<AttachmentType[]> {
    return this.http.get<AttachmentType[]>(`${this.apiUrl}/lookups/attachment-types`);
  }

  // Validate file before upload
  validateFile(file: File): { valid: boolean; error?: string } {
    // Check file size
    if (file.size > ATTACHMENT_CONSTRAINTS.maxFileSize) {
      return {
        valid: false,
        error: 'حجم الملف يتجاوز الحد الأقصى (4 ميجابايت)'
      };
    }

    // Check file type
    if (!ATTACHMENT_CONSTRAINTS.allowedTypes.includes(file.type)) {
      return {
        valid: false,
        error: 'يجب أن يكون الملف بصيغة PDF فقط'
      };
    }

    // Check file extension
    const extension = '.' + file.name.split('.').pop()?.toLowerCase();
    if (!ATTACHMENT_CONSTRAINTS.allowedExtensions.includes(extension)) {
      return {
        valid: false,
        error: 'يجب أن يكون الملف بصيغة PDF فقط'
      };
    }

    return { valid: true };
  }

  // Format file size for display
  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }
}
