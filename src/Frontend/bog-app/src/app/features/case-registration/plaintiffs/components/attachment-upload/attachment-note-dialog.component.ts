import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

export interface AttachmentNoteDialogData {
  fileName: string;
  attachmentTypeName: string;
}

@Component({
  selector: 'app-attachment-note-dialog',
  template: `
    <h2 mat-dialog-title>إضافة ملاحظة للمرفق</h2>
    <mat-dialog-content>
      <div class="file-info">
        <mat-icon>attach_file</mat-icon>
        <span>{{ data.fileName }}</span>
        <span class="attachment-type">({{ data.attachmentTypeName }})</span>
      </div>
      <form [formGroup]="noteForm">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>الملاحظة</mat-label>
          <textarea matInput formControlName="description" rows="3" placeholder="أدخل ملاحظة للمرفق..."></textarea>
          <mat-error *ngIf="noteForm.get('description')?.hasError('required')">
            الملاحظة مطلوبة
          </mat-error>
          <mat-error *ngIf="noteForm.get('description')?.hasError('minlength')">
            الملاحظة يجب أن تكون 3 أحرف على الأقل
          </mat-error>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">إلغاء</button>
      <button mat-raised-button color="primary" (click)="onSave()" [disabled]="noteForm.invalid">
        <mat-icon>upload</mat-icon>
        رفع المرفق
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .file-info {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 12px;
      background: #f5f5f5;
      border-radius: 8px;
      margin-bottom: 16px;
      direction: rtl;
    }

    .file-info mat-icon {
      color: #059669;
    }

    .file-info .attachment-type {
      color: #757575;
      font-size: 12px;
    }

    .full-width {
      width: 100%;
    }

    mat-dialog-content {
      min-width: 400px;
    }

    mat-dialog-actions button {
      margin-right: 8px;
    }
  `]
})
export class AttachmentNoteDialogComponent {
  noteForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<AttachmentNoteDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AttachmentNoteDialogData
  ) {
    this.noteForm = this.fb.group({
      description: ['', [Validators.required, Validators.minLength(3)]]
    });
  }

  onSave(): void {
    if (this.noteForm.valid) {
      this.dialogRef.close(this.noteForm.get('description')?.value);
    }
  }

  onCancel(): void {
    this.dialogRef.close(null);
  }
}
