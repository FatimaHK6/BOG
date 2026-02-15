import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AttachmentApiService, RequestAttachmentVM, RequestAttachmentCreateDTO, AttachmentTypeVM } from '../../services/attachment-api.service';
import { RequestStateService } from '../../services/request-state.service';
import { DeficienciesApiService } from '../../services/deficiencies-api.service';
import { RequestDeficiencyDTO, DeficienciesBatchUpdateDTO } from '../../models/deficiency.model';
import { DeficiencyFormDialogComponent } from '../deficiencies/deficiency-form-dialog.component';

@Component({
  selector: 'app-attachments-list',
  template: `
    <app-section-container
      title="المرفقات"
      sectionId="attachments"
      [showActions]="canEdit">

      <!-- Action buttons in header -->
      <!-- استكمال النواقص FIRST (appears on RIGHT in RTL) -->
      <button slot="header-actions"
              mat-raised-button
              class="deficiency-button"
              (click)="openDeficiencyDialog()"
              *ngIf="canEdit">
        استكمال النواقص
      </button>

      <!-- Add/Cancel buttons AFTER (appear on LEFT in RTL) -->
      <button slot="header-actions"
              mat-raised-button
              class="gold-button"
              (click)="toggleUploadForm()"
              *ngIf="canEdit && !showUploadForm">
        <mat-icon>add</mat-icon>
        إضافة مرفق
      </button>

      <button slot="header-actions"
              mat-button
              (click)="toggleUploadForm()"
              *ngIf="canEdit && showUploadForm">
        <mat-icon>close</mat-icon>
        إلغاء
      </button>

      <div class="attachments-section">
        <!-- Validation Message -->
        <app-validation-message
          *ngIf="showValidation && !attachments.length"
          type="error"
          message="ERR003: المرفقات الإلزامية مفقودة">
        </app-validation-message>

        <!-- File Upload (BR04: PDF only, max 4MB) -->
        <div class="upload-section" *ngIf="canEdit && showUploadForm">
          <!-- Attachment Type Selection -->
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>نوع المرفق *</mat-label>
            <mat-select [value]="selectedAttachmentTypeId" (selectionChange)="selectedAttachmentTypeId = $event.value" required [disabled]="!canEdit">
              <mat-option *ngFor="let type of attachmentTypes" [value]="type.id">
                {{ type.nameAr }}
                <span *ngIf="type.isMandatory" class="mandatory-badge">(إلزامي)</span>
              </mat-option>
            </mat-select>
          </mat-form-field>

          <!-- Description Field -->
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>الوصف (اختياري)</mat-label>
            <textarea matInput [value]="attachmentDescription" (input)="onDescriptionChange($event)" rows="2"></textarea>
          </mat-form-field>

          <input #fileInput type="file" accept=".pdf" (change)="onFileSelected($event)" style="display:none">
          <button mat-raised-button class="gold-button" (click)="fileInput.click()" [disabled]="uploading">
            <mat-icon *ngIf="!uploading">upload_file</mat-icon>
            <mat-spinner *ngIf="uploading" diameter="20" style="display:inline-block;"></mat-spinner>
            {{ uploading ? 'جاري الرفع...' : 'رفع ملف' }}
          </button>
          <small class="help-text">ملفات PDF فقط، الحد الأقصى 4 MB</small>
        </div>

        <!-- Validation Errors -->
        <app-validation-message *ngIf="fileTypeError" type="error" message="BR04: يقبل فقط ملفات PDF">
        </app-validation-message>
        <app-validation-message *ngIf="fileSizeError" type="error" message="BR04: حجم الملف لا يزيد عن 4 MB">
        </app-validation-message>

        <!-- Attachments Table -->
        <div class="attachments-table" *ngIf="attachments.length">
          <table mat-table [dataSource]="attachments" class="mat-elevation-z2">

            <!-- Attachment Type Column -->
            <ng-container matColumnDef="type">
              <th mat-header-cell *matHeaderCellDef>نوع المرفق</th>
              <td mat-cell *matCellDef="let attachment">
                {{ attachment.attachmentTypeName }}
                <span *ngIf="attachment.isMandatory" class="mandatory-badge"> *</span>
              </td>
            </ng-container>

            <!-- Notes/Description Column -->
            <ng-container matColumnDef="notes">
              <th mat-header-cell *matHeaderCellDef>ملاحظات</th>
              <td mat-cell *matCellDef="let attachment">
                {{ attachment.description || '-' }}
              </td>
            </ng-container>

            <!-- Attachment Download Column -->
            <ng-container matColumnDef="attachment">
              <th mat-header-cell *matHeaderCellDef>المرفق</th>
              <td mat-cell *matCellDef="let attachment">
                <button mat-button color="primary" (click)="downloadAttachment(attachment)">
                  <mat-icon>download</mat-icon>
                  {{ attachment.fileName }}
                </button>
                <button mat-icon-button color="warn"
                        (click)="deleteAttachment(attachment)"
                        *ngIf="canEdit"
                        matTooltip="حذف">
                  <mat-icon>delete</mat-icon>
                </button>
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
          </table>
        </div>

        <!-- Empty State -->
        <div class="empty-state" *ngIf="!attachments.length">
          <mat-icon>attach_file</mat-icon>
          <p>لا توجد مرفقات</p>
        </div>
      </div>
    </app-section-container>
  `,
  styles: [`
    .attachments-section { padding: 16px 0; }
    .gold-button { background-color: #C4A35A !important; color: white !important; }
    .gold-button:hover { background-color: #B8963E !important; }
    .deficiency-button { background-color: #C4A35A !important; color: white !important; }
    .deficiency-button:hover { background-color: #B8963E !important; }
    .add-attachment-section { margin: 16px 0; }
    .upload-section { margin: 16px 0; }
    .full-width { width: 100%; margin-bottom: 16px; }
    .help-text { display: block; margin-top: 8px; color: #666; }
    .mandatory-badge { color: red; font-weight: bold; margin-right: 4px; }
    .empty-state {
      text-align: center;
      padding: 40px 20px;
      color: #999;
    }
    .empty-state mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-bottom: 16px;
    }
    .attachments-table {
      margin-top: 20px;
      overflow-x: auto;
    }
    .attachments-table table {
      width: 100%;
    }
    .attachments-table th {
      background-color: #f5f5f5;
      font-weight: 600;
      text-align: right;
      padding: 12px 16px;
    }
    .attachments-table td {
      text-align: right;
      padding: 12px 16px;
      border-bottom: 1px solid #e0e0e0;
    }
  `]
})
export class AttachmentsListComponent implements OnInit, OnDestroy {
  @Input() requestId!: number;
  @Input() canEdit = false;
  @Input() showValidation = false;

  attachments: RequestAttachmentVM[] = [];
  attachmentTypes: AttachmentTypeVM[] = [];
  selectedAttachmentTypeId: number | null = null;
  attachmentDescription: string = '';
  fileTypeError = false;
  fileSizeError = false;
  uploading = false;
  showUploadForm = false;
  displayedColumns: string[] = ['type', 'notes', 'attachment'];

  private destroy$ = new Subject<void>();

  constructor(
    private snackBar: MatSnackBar,
    private attachmentApi: AttachmentApiService,
    private requestState: RequestStateService,
    private dialog: MatDialog,
    private deficienciesApi: DeficienciesApiService
  ) { }

  ngOnInit() {
    this.loadAttachmentTypes();
    this.loadAttachments();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  openDeficiencyDialog() {
    this.openDeficiencyDialogForType(5); // Type 5 = CaseAttachments
  }

  private openDeficiencyDialogForType(typeId: number) {
    const dialogRef = this.dialog.open(DeficiencyFormDialogComponent, {
      width: '600px',
      maxWidth: '95vw',
      direction: 'rtl',
      data: {
        mode: 'create',
        preSelectedTypeId: typeId
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.addDeficiency(result);
      }
    });
  }

  private addDeficiency(newDeficiency: RequestDeficiencyDTO) {
    // Get current deficiencies
    this.deficienciesApi.getDeficiencies(this.requestId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (currentDeficiencies) => {
          // Add new deficiency to existing ones
          const existingDescriptionIds = currentDeficiencies.map(d => ({
            deficiencyDescriptionId: d.deficiencyDescriptionId
          }));

          const updatedDeficiencies = [...existingDescriptionIds, newDeficiency];

          const dto: DeficienciesBatchUpdateDTO = {
            deficiencies: updatedDeficiencies
          };

          this.deficienciesApi.updateDeficiencies(this.requestId, dto)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
              next: () => {
                this.snackBar.open('تم إضافة النقص بنجاح', 'إغلاق', { duration: 3000 });
              },
              error: (error) => {
                console.error('Error adding deficiency:', error);
                this.snackBar.open('فشل في إضافة النقص', 'إغلاق', { duration: 3000 });
              }
            });
        },
        error: (error) => {
          console.error('Error fetching deficiencies:', error);
          this.snackBar.open('فشل في جلب النواقص الحالية', 'إغلاق', { duration: 3000 });
        }
      });
  }

  toggleUploadForm() {
    this.showUploadForm = !this.showUploadForm;
    if (!this.showUploadForm) {
      this.resetForm();
    }
  }

  onDescriptionChange(event: Event) {
    this.attachmentDescription = (event.target as HTMLTextAreaElement).value;
  }

  private loadAttachmentTypes() {
    this.attachmentApi.getAttachmentTypes().subscribe({
      next: (types) => {
        this.attachmentTypes = types;
      },
      error: (error) => {
        console.error('Failed to load attachment types', error);
      }
    });
  }

  private loadAttachments() {
    this.attachmentApi.getAttachments(this.requestId).subscribe({
      next: (attachments) => {
        this.attachments = attachments;
        this.updateAttachmentsCount();
      },
      error: (error) => {
        console.error('Failed to load attachments', error);
      }
    });
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];

      // BR04 validation
      this.fileTypeError = file.type !== 'application/pdf';
      const MAX_FILE_SIZE = 4 * 1024 * 1024;
      this.fileSizeError = file.size > MAX_FILE_SIZE;

      if (!this.fileTypeError && !this.fileSizeError) {
        if (!this.selectedAttachmentTypeId) {
          this.snackBar.open('يرجى تحديد نوع المرفق', 'إغلاق', { duration: 3000 });
          return;
        }
        this.uploadFile(file);
      }
    }
  }

  private uploadFile(file: File) {
    this.uploading = true;

    const reader = new FileReader();
    reader.onload = () => {
      const base64Content = (reader.result as string).split(',')[1];

      const dto: RequestAttachmentCreateDTO = {
        attachmentTypeId: this.selectedAttachmentTypeId!,
        fileName: file.name,
        fileContent: base64Content,
        contentType: file.type,
        description: this.attachmentDescription || undefined
      };

      this.attachmentApi.uploadAttachment(this.requestId, dto).subscribe({
        next: (attachment) => {
          // Reload attachments to ensure proper display with navigation properties
          this.loadAttachments();
          this.uploading = false;
          this.snackBar.open('تم رفع الملف بنجاح', 'إغلاق', { duration: 3000 });
          this.resetForm();
          this.showUploadForm = false;
        },
        error: (error) => {
          this.uploading = false;
          console.error('Upload error:', error);
          this.snackBar.open('فشل رفع الملف', 'إغلاق', { duration: 3000 });
        }
      });
    };

    reader.onerror = () => {
      this.uploading = false;
      this.snackBar.open('خطأ في قراءة الملف', 'إغلاق', { duration: 3000 });
    };

    reader.readAsDataURL(file);
  }

  downloadAttachment(attachment: RequestAttachmentVM) {
    this.attachmentApi.downloadAttachment(this.requestId, attachment.id).subscribe({
      next: (blob) => {
        // Create temporary download link
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = attachment.fileName;
        link.click();
        window.URL.revokeObjectURL(url);

        this.snackBar.open('تم تنزيل المرفق', 'إغلاق', { duration: 2000 });
      },
      error: (error) => {
        console.error('Download error:', error);
        this.snackBar.open('فشل تنزيل المرفق', 'إغلاق', { duration: 3000 });
      }
    });
  }

  deleteAttachment(attachment: RequestAttachmentVM) {
    if (confirm('هل تريد حذف هذا المرفق؟')) {
      this.attachmentApi.deleteAttachment(this.requestId, attachment.id).subscribe({
        next: () => {
          this.attachments = this.attachments.filter(a => a.id !== attachment.id);
          this.updateAttachmentsCount();
          this.snackBar.open('تم حذف المرفق', 'إغلاق', { duration: 3000 });
        },
        error: (error) => {
          console.error('Delete error:', error);
          this.snackBar.open('فشل حذف المرفق', 'إغلاق', { duration: 3000 });
        }
      });
    }
  }

  private updateAttachmentsCount() {
    const count = this.attachments.length;
    this.requestState.updateAttachmentsCount(this.requestId, count);
  }

  private resetForm() {
    this.selectedAttachmentTypeId = null;
    this.attachmentDescription = '';
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i];
  }

  formatDate(date: Date | string): string {
    const d = new Date(date);
    return d.toLocaleDateString('ar-EG');
  }
}
