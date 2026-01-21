import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { RepresentativeVM, RepresentativeAttachmentVM } from '../../../../../core/models/plaintiff.model';
import { RepresentativeType, ALLOWED_REPRESENTATIVE_TYPES } from '../../../../../core/models/representative.model';
import { IdentityType } from '../../../../../core/models/lookup.model';
import { LookupService } from '../../../../../core/services/lookup.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../../shared/components/confirm-dialog/confirm-dialog.component';

export interface RepresentativeDialogData {
  representative?: RepresentativeVM;
  plaintiffId: number;
  plaintiffTypeId: number;
  plaintiffIdentityNumber?: string;  // For ERR012 validation
  existingRepresentatives?: RepresentativeVM[];  // For ERR008 validation
}

@Component({
  selector: 'app-representative-dialog',
  templateUrl: './representative-dialog.component.html',
  styleUrls: ['./representative-dialog.component.scss']
})
export class RepresentativeDialogComponent implements OnInit {
  representativeForm!: FormGroup;
  representativeTypes: RepresentativeType[] = [];
  identityTypes: IdentityType[] = [];
  isLoading = false;
  isLookingUp = false;
  isEditMode = false;

  // Attachment properties
  attachments: RepresentativeAttachmentVM[] = [];
  isDragOver = false;
  isUploading = false;
  readonly MAX_FILE_SIZE = 4 * 1024 * 1024; // 4MB
  readonly ALLOWED_TYPES = ['application/pdf'];

  constructor(
    private fb: FormBuilder,
    private lookupService: LookupService,
    private dialog: MatDialog,
    private notification: NotificationService,
    private dialogRef: MatDialogRef<RepresentativeDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: RepresentativeDialogData
  ) {
    this.isEditMode = !!data.representative;
  }

  ngOnInit(): void {
    this.initForm();
    this.loadLookups();

    if (this.isEditMode && this.data.representative) {
      this.populateForm(this.data.representative);
    }
  }

  private initForm(): void {
    this.representativeForm = this.fb.group({
      representativeTypeId: [null, Validators.required],
      identityTypeId: [1, Validators.required],
      identityNumber: ['', Validators.required],
      firstName: ['', Validators.required],
      fatherName: [''],
      grandfatherName: [''],
      familyName: ['', Validators.required],
      mobileNumber: [''],
      email: ['', Validators.email],
      authorizationNumber: [''],
      authorizationDate: [null],
      authorizationSource: [''],
      authorizationSourceType: [''],
      lawyerLicenseNumber: [''],
      lawyerLicenseDate: [null],
      lawyerLicenseExpiryDate: [null],
      guardianshipType: ['']
    });
  }

  private loadLookups(): void {
    this.lookupService.getRepresentativeTypes().subscribe(types => {
      // Filter representative types based on plaintiff type
      const allowedIds = ALLOWED_REPRESENTATIVE_TYPES[this.data.plaintiffTypeId] || [];
      this.representativeTypes = types.filter(t => allowedIds.includes(t.id));
    });

    this.lookupService.getIdentityTypes().subscribe(types => {
      this.identityTypes = types;
    });
  }

  private populateForm(rep: RepresentativeVM): void {
    this.representativeForm.patchValue({
      representativeTypeId: rep.representativeTypeId,
      identityTypeId: rep.identityTypeId,
      identityNumber: rep.identityNumber,
      firstName: rep.firstName,
      fatherName: rep.fatherName,
      grandfatherName: rep.grandfatherName,
      familyName: rep.familyName,
      mobileNumber: rep.mobileNumber,
      email: rep.email,
      authorizationNumber: rep.authorizationNumber,
      authorizationDate: rep.authorizationDate,
      authorizationSource: rep.authorizationSource,
      authorizationSourceType: rep.authorizationSourceType,
      lawyerLicenseNumber: rep.lawyerLicenseNumber,
      lawyerLicenseDate: rep.lawyerLicenseDate,
      lawyerLicenseExpiryDate: rep.lawyerLicenseExpiryDate,
      guardianshipType: rep.guardianshipType
    });

    // Load existing attachments
    if (rep.attachments) {
      this.attachments = [...rep.attachments];
    }
  }

  onLookupIdentity(): void {
    const identityNumber = this.representativeForm.get('identityNumber')?.value;
    if (!identityNumber) return;

    this.isLookingUp = true;
    // Simulate Absher lookup
    setTimeout(() => {
      // Demo data for testing
      this.representativeForm.patchValue({
        firstName: 'أحمد',
        fatherName: 'محمد',
        grandfatherName: 'عبدالله',
        familyName: 'الشمري'
      });
      this.isLookingUp = false;
    }, 1000);
  }

  onSave(): void {
    if (this.representativeForm.invalid) {
      this.representativeForm.markAllAsTouched();
      return;
    }

    const formValue = this.representativeForm.value;
    const identityNumber = formValue.identityNumber;

    // ERR012: Representative identity cannot be same as plaintiff identity (for Individual type: 1, 2)
    const isIndividualPlaintiff = this.data.plaintiffTypeId === 1 || this.data.plaintiffTypeId === 2;
    if (isIndividualPlaintiff && this.data.plaintiffIdentityNumber && identityNumber === this.data.plaintiffIdentityNumber) {
      this.notification.error('ERR012: لا يمكن إضافة ممثّل بنفس رقم هوية المدّعي');
      return;
    }

    // ERR008: Cannot add representative that already exists for this plaintiff
    if (!this.isEditMode && this.data.existingRepresentatives) {
      const existingRep = this.data.existingRepresentatives.find(r => r.identityNumber === identityNumber);
      if (existingRep) {
        this.notification.error('ERR008: الممثّل موجود مسبقاً للمدّعي على الطلب');
        return;
      }
    }

    const fullName = [formValue.firstName, formValue.fatherName, formValue.grandfatherName, formValue.familyName]
      .filter(n => n)
      .join(' ');

    const representative: any = {
      ...formValue,
      fullName,
      plaintiffId: this.data.plaintiffId,
      attachments: this.attachments
    };

    if (this.isEditMode && this.data.representative) {
      representative.id = this.data.representative.id;
    }

    this.dialogRef.close(representative);
  }

  onCancel(): void {
    if (this.representativeForm.dirty) {
      const dialogData: ConfirmDialogData = {
        title: 'تغييرات غير محفوظة',
        message: 'لديك تغييرات غير محفوظة. هل أنت متأكد من المغادرة بدون حفظ؟',
        confirmText: 'مغادرة',
        cancelText: 'البقاء',
        confirmColor: 'warn',
        icon: 'warning'
      };

      const confirmDialogRef = this.dialog.open(ConfirmDialogComponent, {
        width: '400px',
        data: dialogData
      });

      confirmDialogRef.afterClosed().subscribe(result => {
        if (result) {
          this.dialogRef.close();
        }
      });
    } else {
      this.dialogRef.close();
    }
  }

  // Check if authorization fields should be shown
  showAuthorizationFields(): boolean {
    const typeId = this.representativeForm.get('representativeTypeId')?.value;
    // Agent/Lawyer (1) requires authorization/power of attorney
    return typeId === 1;
  }

  // Check if guardianship type should be shown
  showGuardianshipType(): boolean {
    const typeId = this.representativeForm.get('representativeTypeId')?.value;
    // Guardian (2) requires guardianship type
    return typeId === 2;
  }

  // ========== Attachment Methods ==========

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
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

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.processFile(files[0]);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.processFile(input.files[0]);
      input.value = ''; // Reset for re-selecting same file
    }
  }

  private processFile(file: File): void {
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

    // Directly add attachment without notes dialog
    this.addAttachment(file);
  }

  private addAttachment(file: File): void {
    const newAttachment: RepresentativeAttachmentVM = {
      id: Date.now(), // Temporary ID
      representativeId: 0,
      attachmentTypeId: 10, // Representation document type
      attachmentTypeName: 'Representation Document',
      attachmentTypeNameAr: 'صورة التمثيل',
      fileName: file.name,
      fileSizeBytes: file.size,
      contentType: file.type,
      downloadUrl: URL.createObjectURL(file),
      uploadDate: new Date(),
      file: file
    };

    this.attachments = [...this.attachments, newAttachment];

    this.notification.success('تم إضافة المرفق');
  }

  onViewAttachment(attachment: RepresentativeAttachmentVM): void {
    if (attachment.downloadUrl) {
      window.open(attachment.downloadUrl, '_blank');
    }
  }

  onDeleteAttachment(index: number): void {
    const dialogData: ConfirmDialogData = {
      title: 'تأكيد الحذف',
      message: `هل أنت متأكد من حذف المرفق "${this.attachments[index].fileName}"؟`,
      confirmText: 'حذف',
      cancelText: 'إلغاء',
      confirmColor: 'warn',
      icon: 'delete'
    };

    const confirmDialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: dialogData
    });

    confirmDialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.attachments = this.attachments.filter((_, i) => i !== index);
        this.notification.success('تم حذف المرفق');
      }
    });
  }

  formatFileSize(bytes: number): string {
    if (!bytes || isNaN(bytes)) return '0 B';
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }
}
