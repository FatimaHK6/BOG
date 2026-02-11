import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges, ViewChild, ElementRef } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { HttpEventType } from '@angular/common/http';
import { PlaintiffAttachmentVM } from '../../../../../core/models/plaintiff.model';
import { AttachmentService } from '../../../../../core/services/attachment.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { AttachmentNoteDialogComponent } from './attachment-note-dialog.component';

export interface AttachmentType {
  id: number;
  name: string;
  nameAr: string;
  required: boolean;
  maxCount?: number; // BC01: Maximum number of files for this attachment type
}

// Required attachments by plaintiff type (BC01)
export const REQUIRED_ATTACHMENTS: { [key: number]: AttachmentType[] } = {
  1: [{ id: 1, name: 'Identity Copy', nameAr: 'صورة الهوية', required: true, maxCount: 1 }], // Individual (فرد)
  2: [{ id: 1, name: 'Identity Copy', nameAr: 'صورة الهوية', required: false, maxCount: 1 }], // Individual without ID (فرد بدون هوية)
  3: [
    { id: 1, name: 'Identity Copy', nameAr: 'صورة الهوية', required: true, maxCount: 1 },
    { id: 2, name: 'Commercial Registration', nameAr: 'السجل التجاري', required: true, maxCount: 1 }
  ], // Business Owner (صاحب مؤسسة)
  4: [{ id: 2, name: 'Commercial Registration', nameAr: 'السجل التجاري', required: true, maxCount: 1 }], // Registered Company (شركة مسجلة)
  5: [{ id: 3, name: 'Company Contract', nameAr: 'عقد الشركة', required: true, maxCount: 1 }], // Unregistered Company (شركة غير مسجلة)
  6: [{ id: 4, name: 'Representation Decision', nameAr: 'قرار التمثيل', required: true, maxCount: 1 }], // Government Agency (جهة حكومية)
  7: [{ id: 5, name: 'Organization License', nameAr: 'رخصة المنظمة', required: true, maxCount: 1 }], // NGO/Charity (جمعية/مؤسسة خيرية)
  8: [{ id: 6, name: 'Waqf Deed', nameAr: 'صك الوقف', required: true, maxCount: 1 }] // Waqf (وقف)
};

// Default max count when not specified
export const DEFAULT_MAX_COUNT = 1;

@Component({
  selector: 'app-attachment-upload',
  templateUrl: './attachment-upload.component.html',
  styleUrls: ['./attachment-upload.component.scss']
})
export class AttachmentUploadComponent implements OnInit, OnChanges {
  @ViewChild('fileInput', { static: false }) fileInput!: ElementRef<HTMLInputElement>;

  @Input() plaintiffId: number = 0;
  @Input() plaintiffTypeId: number = 1;
  @Input() attachments: PlaintiffAttachmentVM[] = [];
  @Input() readonly: boolean = false;
  @Output() attachmentsChange = new EventEmitter<PlaintiffAttachmentVM[]>();

  requiredAttachmentTypes: AttachmentType[] = [];
  isDragOver = false;
  currentAttachmentTypeId: number = 1;

  // INF01: Required attachments info message
  inf01Message: string = '';

  readonly MAX_FILE_SIZE = 4 * 1024 * 1024; // 4MB
  readonly ALLOWED_TYPES = ['application/pdf'];

  uploadProgress: { [key: number]: number } = {};
  isUploading = false;

  constructor(
    private notification: NotificationService,
    private dialog: MatDialog,
    private attachmentService: AttachmentService
  ) {}

  ngOnInit(): void {
    this.updateRequiredAttachments();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['plaintiffTypeId']) {
      this.updateRequiredAttachments();
    }
  }

  private updateRequiredAttachments(): void {
    this.requiredAttachmentTypes = REQUIRED_ATTACHMENTS[this.plaintiffTypeId] || [];
    this.generateINF01Message();
  }

  // INF01: Generate info message about required attachments based on plaintiff type (BC01)
  // Format: {sequential number} - {attachment type} بحد أقصى ({max count})
  private generateINF01Message(): void {
    const requiredTypes = this.requiredAttachmentTypes.filter(t => t.required);

    if (requiredTypes.length === 0) {
      this.inf01Message = '';
      return;
    }

    const items = requiredTypes.map((type, index) => {
      const maxCount = type.maxCount || DEFAULT_MAX_COUNT;
      return `${index + 1}- ${type.nameAr} بحد أقصى (${maxCount})`;
    });

    this.inf01Message = items.join('\n');
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    if (!this.readonly) {
      this.isDragOver = true;
    }
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;

    if (this.readonly) {
      return;
    }

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.processFile(files[0]);
    }
  }

  // Opens file dialog for specific attachment type
  openFileDialog(attachmentTypeId: number): void {
    this.currentAttachmentTypeId = attachmentTypeId;
    if (this.fileInput?.nativeElement) {
      this.fileInput.nativeElement.value = ''; // Reset to allow re-selecting same file
      this.fileInput.nativeElement.click();
    }
  }

  // Handles file input change event
  onFileInputChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.processFile(input.files[0], this.currentAttachmentTypeId);
      input.value = ''; // Reset after processing
    }
  }

  private processFile(file: File, attachmentTypeId?: number): void {
    // Validate file type
    if (!this.ALLOWED_TYPES.includes(file.type)) {
      this.notification.validation('يجب أن يكون الملف بصيغة PDF فقط');
      return;
    }

    // Validate file size
    if (file.size > this.MAX_FILE_SIZE) {
      this.notification.validation('حجم الملف يتجاوز الحد الأقصى (4 ميجابايت)');
      return;
    }

    const typeId = attachmentTypeId || this.requiredAttachmentTypes[0]?.id || 1;
    const attachmentType = this.requiredAttachmentTypes.find(t => t.id === typeId);

    // Open dialog to get the required note
    const dialogRef = this.dialog.open(AttachmentNoteDialogComponent, {
      width: '500px',
      data: {
        fileName: file.name,
        attachmentTypeName: attachmentType?.nameAr || 'مرفق'
      },
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(description => {
      if (description) {
        // If plaintiff already exists, upload to API
        if (this.plaintiffId > 0) {
          this.uploadToApi(file, typeId, attachmentType, description);
        } else {
          // For new plaintiffs, store locally until plaintiff is saved
          this.storeLocally(file, typeId, attachmentType, description);
        }
      }
    });
  }

  private uploadToApi(file: File, typeId: number, attachmentType: AttachmentType | undefined, description: string): void {
    this.isUploading = true;
    this.uploadProgress[typeId] = 0;

    this.attachmentService.uploadAttachment(this.plaintiffId, typeId, file, description).subscribe({
      next: (event) => {
        if (event.type === HttpEventType.UploadProgress && event.total) {
          this.uploadProgress[typeId] = Math.round(100 * event.loaded / event.total);
        } else if (event.type === HttpEventType.Response) {
          const uploadedAttachment = event.body as PlaintiffAttachmentVM;

          // Update local list
          const existingIndex = this.attachments.findIndex(a => a.attachmentTypeId === typeId);
          if (existingIndex >= 0) {
            this.attachments[existingIndex] = uploadedAttachment;
          } else {
            this.attachments.push(uploadedAttachment);
          }

          this.attachmentsChange.emit([...this.attachments]);
          this.isUploading = false;
          delete this.uploadProgress[typeId];

          this.notification.success('تم رفع المرفق بنجاح');
        }
      },
      error: (error) => {
        this.isUploading = false;
        delete this.uploadProgress[typeId];
        console.error('Upload error:', error);
        this.notification.handleError(error, 'فشل في رفع المرفق');
      }
    });
  }

  private storeLocally(file: File, typeId: number, attachmentType: AttachmentType | undefined, description: string): void {
    const newAttachment: PlaintiffAttachmentVM = {
      id: Date.now(), // Temporary ID
      plaintiffId: 0,
      attachmentTypeId: typeId,
      attachmentTypeName: attachmentType?.name || 'Document',
      attachmentTypeNameAr: attachmentType?.nameAr || 'مستند',
      fileName: file.name,
      fileSizeBytes: file.size,
      contentType: file.type,
      downloadUrl: URL.createObjectURL(file),
      uploadDate: new Date(),
      description: description, // User note/comment
      file: file // Store file for later upload
    };

    // Check if attachment of this type already exists
    const existingIndex = this.attachments.findIndex(a => a.attachmentTypeId === typeId);
    if (existingIndex >= 0) {
      this.attachments[existingIndex] = newAttachment;
    } else {
      this.attachments.push(newAttachment);
    }

    this.attachmentsChange.emit([...this.attachments]);

    this.notification.info('تم إضافة المرفق (سيتم الرفع عند حفظ المدعي)');
  }

  getUploadProgress(typeId: number): number {
    return this.uploadProgress[typeId] || 0;
  }

  isTypeUploading(typeId: number): boolean {
    return this.uploadProgress[typeId] !== undefined;
  }

  onDeleteAttachment(attachment: PlaintiffAttachmentVM): void {
    const dialogData: ConfirmDialogData = {
      title: 'تأكيد الحذف',
      message: `هل أنت متأكد من حذف المرفق "${attachment.fileName}"؟`,
      confirmText: 'حذف',
      cancelText: 'إلغاء',
      confirmColor: 'warn',
      icon: 'delete'
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: dialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        const index = this.attachments.findIndex(a => a.id === attachment.id);
        if (index >= 0) {
          this.attachments.splice(index, 1);
          this.attachmentsChange.emit([...this.attachments]);

          this.notification.success('تم حذف المرفق');
        }
      }
    });
  }

  onViewAttachment(attachment: PlaintiffAttachmentVM): void {
    if (attachment.downloadUrl) {
      window.open(attachment.downloadUrl, '_blank');
    }
  }

  getAttachmentForType(typeId: number): PlaintiffAttachmentVM | undefined {
    return this.attachments.find(a => a.attachmentTypeId === typeId);
  }

  isTypeUploaded(typeId: number): boolean {
    return this.attachments.some(a => a.attachmentTypeId === typeId);
  }

  formatFileSize(bytes: number): string {
    if (!bytes || isNaN(bytes)) return '0 B';
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }

  getAttachmentStatus(): 'complete' | 'incomplete' {
    const required = this.requiredAttachmentTypes.filter(t => t.required);
    const uploaded = required.every(t => this.isTypeUploaded(t.id));
    return uploaded ? 'complete' : 'incomplete';
  }
}
